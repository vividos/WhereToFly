using SkiaSharp.Views.Maui.Controls.Hosting;

namespace WhereToFly.App.Svg
{
    /// <summary>
    /// Extension methods for MauiAppBuilder for the <see cref="SvgImage"/> control.
    /// </summary>
    public static class SvgImageMauiAppBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="SvgImage"/> to MAUI to be usable in the user interface.
        /// </summary>
        /// <param name="builder">the app builder</param>
        /// <returns>also the app builder</returns>
        public static MauiAppBuilder UseSvgImage(this MauiAppBuilder builder)
        {
            return builder.UseSkiaSharp();
        }
    }
}
