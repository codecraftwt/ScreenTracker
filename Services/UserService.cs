using ScreenTracker1.DTOS;
using ScreenTracker1.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace ScreenTracker1.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private NavigationManager? _navigationManager;

        private bool _isPopupShown = false;
        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        
        }
        public void SetNavigationManager(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }


        private async void HandleUnauthorized()
        {
            if (_isPopupShown) return;

            _isPopupShown = true;
            Preferences.Remove("authToken");

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await App.Current.MainPage.DisplayAlert(
                    "Session Expired",
                    "You have logged in from another device. Please log in again.",
                    "OK"
                );

                _navigationManager?.NavigateTo("/login", forceLoad: true);

                _isPopupShown = false;
            });
        }


        private void AddAuthorizationHeader(HttpRequestMessage request)
        {
            string? token = Preferences.Get("authToken", "");
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Console.WriteLine("JWT token is missing. User might not be logged in.");
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{App.URL}User/allUsers");
                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    return new List<User>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<User>>(responseString);
                return users ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<List<AppUsage>> GetAppUsageByUserIdAsync(int userId, DateTime date, int page = 1, int take = 5)
        {
            try
            {
                string formattedDate = date.ToString("yyyy-MM-dd");
                string url = $"{App.URL}AppUsage/day/{formattedDate}/{userId}?page={page}&take={take}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        HandleUnauthorized();
                    }

                    Console.WriteLine($"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    return new List<AppUsage>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var appUsageList = JsonSerializer.Deserialize<List<AppUsage>>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return appUsageList ?? new List<AppUsage>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching app usage data: {ex.Message}");
                return new List<AppUsage>();
            }
        }

        public async Task<List<AppUsageData>> GetAppUsageDataAsync(int id)
        {
            try
            {
                var url = $"{App.URL}AppUsage/lastDaysTotal/{id}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    HandleUnauthorized();
                    Console.WriteLine($"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    return new List<AppUsageData>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var appUsageData = JsonSerializer.Deserialize<List<AppUsageData>>(responseString);
                return appUsageData ?? new List<AppUsageData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching app usage data with ID {id}: {ex.Message}");
                return new List<AppUsageData>();
            }
        }

        public async Task<List<AppTitle>> GetAppTitleByDateAsync(DateTime date, int id, int page = 1, int take = 5)
        {
            try
            {
                string formattedDate = date.ToString("yyyy-MM-dd");
                var url = $"{App.URL}AppTitle/day/{formattedDate}/{id}?page={page}&take={take}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    HandleUnauthorized();
                    Console.WriteLine($"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    return new List<AppTitle>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var appTitleList = JsonSerializer.Deserialize<List<AppTitle>>(responseString);
                return appTitleList ?? new List<AppTitle>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching app title data: {ex.Message}");
                return new List<AppTitle>();
            }
        }

        public async Task<List<CategoryKeywordGroup>> GetGroupedCategoryKeywordsAsync(int id, DateTime date, int page = 1, int take = 5)
        {
            try
            {
                string formattedDate = date.ToString("yyyy-MM-dd");
                var url = $"{App.URL}CategoryKeyword/groupedCategory/{id}/{formattedDate}?page={page}&take={take}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    HandleUnauthorized();
                    Console.WriteLine($"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    return new List<CategoryKeywordGroup>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var groupedKeywords = JsonSerializer.Deserialize<List<CategoryKeywordGroup>>(responseString);
                return groupedKeywords ?? new List<CategoryKeywordGroup>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching grouped category keywords: {ex.Message}");
                return new List<CategoryKeywordGroup>();
            }
        }

        public async Task<List<AppUsage>> GetAppUsageByUserIdAsyncuser(int userId, DateTime startDate, int page = 1, int take = 5)
        {
            try
            {
                string formattedDate = startDate.ToString("yyyy-MM-dd");
                var url = $"{App.URL}AppUsage/user/usage?startDate={formattedDate}&page={page}&take={take}";


                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    HandleUnauthorized();
                    Console.WriteLine($"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    return new List<AppUsage>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var appUsageList = JsonSerializer.Deserialize<List<AppUsage>>(responseString);
                return appUsageList ?? new List<AppUsage>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching app usage data: {ex.Message}");
                return new List<AppUsage>();
            }
        }

        public async Task<List<AppTitle>> GetAppTitleDetailsAsync(DateTime date, int userId, string appName, int page = 1, int take = 5)
        {
            try
            {
                string formattedDate = date.ToString("yyyy-MM-dd");
                string url = $"{App.URL}AppTitle/AppDetails/{formattedDate}/{userId}/{appName}?page={page}&take={take}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        HandleUnauthorized();
                    }

                    Console.WriteLine($"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    return new List<AppTitle>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var appTitleList = JsonSerializer.Deserialize<List<AppTitle>>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return appTitleList ?? new List<AppTitle>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching app title details: {ex.Message}");
                return new List<AppTitle>();
            }
        }

        public async Task<List<CategoryKeywordGroup>> GetCategoryByAppNameAsync(string appName)
        {
            try
            {
                var url = $"{App.URL}CategoryKeyword/Category/{appName}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    HandleUnauthorized();
                    Console.WriteLine($"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    return new List<CategoryKeywordGroup>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<CategoryKeywordGroup>>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new List<CategoryKeywordGroup>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching category by app name: {ex.Message}");
                return new List<CategoryKeywordGroup>();
            }
        }

        public async Task<List<Screenshots>> GetImagesByDateAsync(int userId, DateTime date, int skip = 1, int take = 6)
        {
            try
            {
                string formattedDate = date.ToString("yyyy-MM-dd");
                string url = $"{App.URL}Image/by-date?userId={userId}&date={formattedDate}&skip={skip}&take={take}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    return new List<Screenshots>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var images = JsonSerializer.Deserialize<List<Screenshots>>(responseString);

                if (images != null)
                {
                    //foreach (var image in images)
                    //{
                    //    string fullImageUrl = $"{App.ImgURL}{image.imageUrl}";
                    //    Console.WriteLine($"Full Image URL: {fullImageUrl}"); 

                    //    image.imageUrl = fullImageUrl;

                    //    return images ?? new List<Screenshots>();
                    //}

                    
                }
                return images ?? new List<Screenshots>();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching images: {ex.Message}");
                return new List<Screenshots>();
            }
        }




        public async Task<bool>DeleteScreenshotAsync(int screenshotId)
        {
            try
            {
                string url = $"{App.URL}Image/{screenshotId}";
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                AddAuthorizationHeader(request);
                var response = await _httpClient.SendAsync(request);

                if(!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HTTP{response.StatusCode}:{await response.Content.ReadAsStringAsync()}");
                    return false;
                }
                return true;

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error deleting screenshot: {ex.Message}");
                return false;
            }
        }

        public async Task<double> GetAfkLogsTotalAsync(int userId, DateTime date)
        {
            try
            {
                string formattedDate = date.ToString("yyyy-MM-dd");
                string url = $"{App.URL}AfkLogs/total?userId={userId}&date={formattedDate}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddAuthorizationHeader(request);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        HandleUnauthorized();
                    }

                    Console.WriteLine($"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    return 0;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                string timeText = responseString.Trim('"');

                if (TimeSpan.TryParse(timeText, out TimeSpan ts))
                {
                    return ts.TotalMinutes; 
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching AFK total logs: {ex.Message}");
                return 0;
            }
        }



    }
}
