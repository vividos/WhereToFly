using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
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
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseSkiaSharp()
                .UseFFImageLoading()
                .UseMapView();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
