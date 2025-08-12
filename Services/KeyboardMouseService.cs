using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ScreenTracker1.Models;

namespace ScreenTracker1.Services
{
    public class KeyboardMouseService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;

        public KeyboardMouseService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(App.URL);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<Screenshots> GetImageActivityDataAsync(int imageId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Image/{imageId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Screenshots>(content, _serializerOptions);  
                }

                return null; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching image activity data: {ex.Message}");
                return null; 
            }
        }



        public Dictionary<string, MinuteActivity> ParseMinuteActivityData(string minuteActivityData)
        {
            try
            {
                if (!string.IsNullOrEmpty(minuteActivityData))
                {
              
                    return JsonSerializer.Deserialize<Dictionary<string, MinuteActivity>>(minuteActivityData, _serializerOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing minute activity data: {ex.Message}");
                return null;
            }
        }
    }
}
