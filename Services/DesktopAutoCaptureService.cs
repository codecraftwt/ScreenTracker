using Microsoft.Maui.Controls.PlatformConfiguration;
using ScreenTracker1.Platforms.Windows;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Gma.System.MouseKeyHook;
using System.Text.Json;

namespace ScreenTracker1.Services
{
    public class DesktopAutoCaptureService
    {
        private readonly DesktopScreenshotService _screenshotService;
        private readonly HttpClient _httpClient;
        private System.Timers.Timer _oneMinuteTimer;
        private System.Timers.Timer _threeMinuteTimer;
        private int _userId;
        private int _currentMinute = 0;

        private int _keyboardClicks = 0;
        private int _mouseClicks = 0;
    
        private Dictionary<int, (int keyboard, int mouse, DateTime timestamp)> _minuteStats = new Dictionary<int, (int, int, DateTime)>();

        private IKeyboardMouseEvents _keyboardMouseEvents;
        private bool _isFirstMinute = true;


        private DateTime _lastActivityTime = DateTime.Now;
        private bool _isAfk = false;
        private const int AfkThresholdMinutes =5;

        public DesktopAutoCaptureService(DesktopScreenshotService screenshotService)
        {
            _screenshotService = screenshotService;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(App.URL)
            };
        }

        private string token;

        public void Start()
        {
            _keyboardClicks = 0;
            _mouseClicks = 0;
            _currentMinute = 0;
            _minuteStats.Clear();
            _isFirstMinute = true;

            _keyboardMouseEvents = Hook.GlobalEvents();
            _keyboardMouseEvents.KeyDown += (s, e) => _keyboardClicks++;
            _keyboardMouseEvents.MouseDown += (s, e) => _mouseClicks++;

            token = Preferences.Get("authToken", null);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var userIdClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (int.TryParse(userIdClaim, out int userId))
            {
                _userId = userId;
            }

            // 1-minute timer to track activity
            _oneMinuteTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _oneMinuteTimer.Elapsed += OnOneMinuteElapsed;
            _oneMinuteTimer.AutoReset = true;
            _oneMinuteTimer.Enabled = true;

            // 3-minute timer for screenshots
            _threeMinuteTimer = new System.Timers.Timer(TimeSpan.FromMinutes(3).TotalMilliseconds);
            _threeMinuteTimer.Elapsed += async (s, e) => await CaptureAndSendAsync();
            _threeMinuteTimer.AutoReset = true;
            _threeMinuteTimer.Enabled = true;
        }

        private void OnOneMinuteElapsed(object sender, ElapsedEventArgs e)
        {
            _minuteStats[_currentMinute] = (_keyboardClicks, _mouseClicks, DateTime.Now);

            _keyboardClicks = 0;
            _mouseClicks = 0;

            _currentMinute = (_currentMinute + 1) % 3;

            if (_currentMinute == 0)
            {
                _isFirstMinute = false;
            }
        }


        public void StopTimer()
        {
            _oneMinuteTimer?.Stop();
            _oneMinuteTimer?.Dispose();

            _threeMinuteTimer?.Stop();
            _threeMinuteTimer?.Dispose();

            _keyboardMouseEvents?.Dispose();
        }

        private async Task CaptureAndSendAsync()
        {
            if (_isFirstMinute) return;

            try
            {
                // Ensure we have all 3 minutes of data
                if (_minuteStats.Count < 3)
                {
                    Console.WriteLine("Not enough minute data collected yet");
                    return;
                }

                var imageBytes = _screenshotService.CaptureDesktop();
                var content = new MultipartFormDataContent();
                var byteContent = new ByteArrayContent(imageBytes);
                byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");

                content.Add(byteContent, "file", $"desktop_screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                content.Add(new StringContent(_userId.ToString()), "userId");

                // Calculate totals
                int totalKeyboard = _minuteStats.Values.Sum(m => m.keyboard);
                int totalMouse = _minuteStats.Values.Sum(m => m.mouse);

                // Create minute data dictionary with proper value tuples
                var minuteData = new Dictionary<string, Dictionary<string, object>>();
                for (int i = 0; i < 3; i++)
                {
                    if (_minuteStats.TryGetValue(i, out var stats))
                    {
                        minuteData[$"Minute{i + 1}"] = new Dictionary<string, object>
        {
            { "Keyboard", stats.keyboard },
            { "Mouse", stats.mouse },
            { "Timestamp", stats.timestamp.ToString("o") }
        };
                    }
                }



                // Serialize to JSON
                var minuteActivityJson = JsonSerializer.Serialize(minuteData);

                content.Add(new StringContent(totalKeyboard.ToString()), "keyboardClicks");
                content.Add(new StringContent(totalMouse.ToString()), "mouseClicks");
                content.Add(new StringContent(minuteActivityJson), "minuteActivityData");

                var response = await _httpClient.PostAsync("Image/upload", content);
                response.EnsureSuccessStatusCode();

                // Reset for next cycle
                _minuteStats.Clear();
                _currentMinute = 0;
                _isFirstMinute = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing/sending screenshot: {ex.Message}");
            }
        }
    }
}