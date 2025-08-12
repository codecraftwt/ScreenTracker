using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace ScreenTracker1.Services
{
    public class AuthTokenService
    {
        private readonly IJSRuntime _jsRuntime;

        public AuthTokenService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        
        public async Task<string> GetTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        }
    }
}