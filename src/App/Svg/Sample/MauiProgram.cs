namespace WhereToFly.App.Svg.Sample
{
    /// <summary>
    /// MAUI sample program
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
                .UseSvgImage();

            return builder.Build();
        }
    }
}
