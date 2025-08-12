using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ScreenTracker1.Models;

namespace ScreenTracker1.Services
{
    public class LoginService
    {
        private readonly HttpClient _httpClient;

        public LoginService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> LoginAsync(LoginModel loginModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{App.URL}Auth/login", loginModel);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return result; 
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                   
                    return "Invalid credentials. Please check your username and password.";
                }
                else
                {
                    return "Login failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

    }
}
