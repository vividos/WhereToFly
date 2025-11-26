using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace WhereToFly.App.MapView;

/// <summary>
/// Extension methods for MauiAppBuilder for the <see cref="MapView"/> control.
/// </summary>
public static class MapViewMauiAppBuilderExtensions
{
    /// <summary>
    /// Adds <see cref="MapView"/> to MAUI to be usable in the user interface.
    /// </summary>
    /// <param name="builder">the app builder</param>
    /// <returns>also the app builder</returns>
    public static MauiAppBuilder UseMapView(this MauiAppBuilder builder)
    {
        return builder
            .ConfigureMauiHandlers((handlers) =>
             {
#if ANDROID || WINDOWS
                 handlers.AddHandler<MapView, MapViewHandler>();
                 handlers.AddHandler<HeightProfileView, MapViewHandler>();
#endif

#if ANDROID
                 // Note: Using AppendToMapping() instead of ModifyMapping(), since this would
                 // interfere with other handlers, despite suggested here:
                 // https://github.com/dotnet/maui/pull/16032
                 MapViewHandler.Mapper.AppendToMapping(
                     "MapView-" + nameof(Android.Webkit.WebViewClient),
                     MapViewHandler.SetupWebViewClient);
#endif
             });
    }
}
