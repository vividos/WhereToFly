using System.IO;
using Xamarin.Forms;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Platform abstraction interface
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        /// Base path to use in WebView control
        /// </summary>
        string WebViewBasePath { get; }

        /// <summary>
        /// Opens asset stream and returns it
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>stream to read from file</returns>
        Stream OpenAssetStream(string assetFilename);

        /// <summary>
        /// Loads text of asset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>text content of asset</returns>
        string LoadAssetText(string assetFilename);

        /// <summary>
        /// Sets app theme to use for platform
        /// </summary>
        /// <param name="requestedTheme">requested theme</param>
        void SetPlatformTheme(OSAppTheme requestedTheme);

        /// <summary>
        /// Translates the compass' magnetic north heading (e.g. from Xamarin.Essentials.Compass
        /// API) to true north.
        /// </summary>
        /// <param name="headingMagneticNorthInDegrees">magnetic north heading</param>
        /// <param name="latitudeInDegrees">latitude of current position</param>
        /// <param name="longitudeInDegrees">longitude of current position</param>
        /// <param name="altitudeInMeter">altitude of current position</param>
        /// <param name="headingTrueNorthInDegrees">true north heading</param>
        /// <returns>true when tralslating was successful, false when not available</returns>
        bool TranslateCompassMagneticNorthToTrueNorth(
            int headingMagneticNorthInDegrees,
            double latitudeInDegrees,
            double longitudeInDegrees,
            double altitudeInMeter,
            out int headingTrueNorthInDegrees);
    }
}
