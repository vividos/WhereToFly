using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using WhereToFly.App.Weather.Abstractions;
using WhereToFly.App.Weather.Services;
using WhereToFly.App.Weather.Views;

namespace WhereToFly.App.Weather;

/// <summary>
/// MAUI program
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates a MAUI app object
    /// </summary>
    /// <returns>MAUI app object</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

        builder = WeatherWebView.UseWeatherWebView(builder);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<IFaviconDataService, FaviconDataService>();
        builder.Services.AddSingleton<IWeatherDashboardIconDataService, WeatherDashboardIconDataService>();
        builder.Services.AddSingleton<WeatherIconDescriptionRepository>();

#if ANDROID
        builder.Services.AddSingleton<IAppManager, Platforms.Android.AndroidAppManager>();
#elif WINDOWS
        builder.Services.AddSingleton<IAppManager, Platforms.Windows.WindowsAppManager>();
#endif

        return builder.Build();
    }
}
