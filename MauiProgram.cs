using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using MudBlazor.Services;
using ScreenTracker1.Services;
using ScreenTracker1.Platforms.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace ScreenTracker1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif
            builder.ConfigureMauiHandlers(handlers =>
            {
              
            });

            builder.Services.AddMudServices();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddScoped<LoginService>();
            builder.Services.AddScoped<AfkTrackerService>();
            builder.Services.AddSingleton<AppUsageTracker>();

#if WINDOWS
            builder.Services.AddSingleton<DesktopScreenshotService>();
            builder.Services.AddSingleton<DesktopAutoCaptureService>();
            builder.Services.AddSingleton<KeyboardMouseService>();
#endif

            builder.Services.AddScoped(sp => new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(30)
            });

            return builder.Build();
        }
    }
}
