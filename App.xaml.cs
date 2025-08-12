using Microsoft.IdentityModel.Tokens;
using Microsoft.Maui.Controls;
using ScreenTracker1.Services;

namespace ScreenTracker1;

public partial class App : Application
{

    public static string ImgURL { get; set; } = "https://90a33e7024ad.ngrok-free.app";

    public static string URL { get; set; } = $"{App.ImgURL}/api/";
   
   

    
    
    public static int UserID { get; set; } = 0;
    




    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
            var tracker = serviceProvider.GetService<AppUsageTracker>();


        MainPage = new MainPage();
     
        //_screenshotService.Start();
        //_screenshotService = screenshotService;
    }
}

