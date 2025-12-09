using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using WhereToFly.App.Abstractions;
using WhereToFly.App.Controls;
using WhereToFly.App.MapView;
using WhereToFly.App.Services;

namespace WhereToFly.App
{
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
#if WINDOWS
                .UseMauiCommunityToolkit(
                    options =>
                    {
                        options.SetShouldEnableSnackbarOnWindows(true);
                    })
#elif ANDROID
                .UseMauiCommunityToolkit()
#endif
#if WINDOWS
                .ConfigureEssentials(essentials =>
                {
                    essentials.UseMapServiceToken(Constants.BingMapsKeyWindows);
                })
#endif
                .UseSkiaSharp()
                .UseMapView();

            builder = WeatherWebView.UseWeatherWebView(builder);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<IUserInterface, UserInterface>();
            builder.Services.AddSingleton<IAppMapService, AppMapService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<IDataService, Services.SqliteDatabase.SqliteDatabaseDataService>();
            builder.Services.AddSingleton<IGeolocationService, GeolocationService>();
            builder.Services.AddSingleton<CompassGeoServices>();
            builder.Services.AddSingleton<LiveDataRefreshService>();

#if ANDROID
            builder.Services.AddSingleton<IAppManager, Platforms.Android.AndroidAppManager>();
#elif WINDOWS
            builder.Services.AddSingleton<IAppManager, Platforms.Windows.WindowsAppManager>();
#endif

            return builder.Build();
        }
    }
}
