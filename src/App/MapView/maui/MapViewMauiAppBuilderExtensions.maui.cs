using Microsoft.Maui.Hosting;

namespace WhereToFly.App.MapView
{
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
                     handlers.AddHandler(typeof(MapView), typeof(MapViewHandler));
                 });
        }
    }
}
