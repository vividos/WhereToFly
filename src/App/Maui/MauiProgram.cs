using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using WhereToFly.App.Controls;
using WhereToFly.App.MapView;

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

#pragma warning disable CA1416 // Validate platform compatibility
            builder
                .UseMauiApp<App>()
#if WINDOWS
                .UseMauiCommunityToolkit(
                    options =>
                    {
                        options.SetShouldEnableSnackbarOnWindows(true);
                    })
#else
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
#pragma warning restore CA1416 // Validate platform compatibility

            builder = WeatherWebView.UseWeatherWebView(builder);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
