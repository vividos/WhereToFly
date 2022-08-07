using System.IO;
using WhereToFly.App.Core;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// IPlatform implementation for unit tests
    /// </summary>
    internal class UnitTestPlatform : IPlatform
    {
        /// <summary>
        /// Returns web view base path; always "about:blank"
        /// </summary>
        public string WebViewBasePath => "about:blank";

        /// <summary>
        /// Loads text document from assets
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>asset text</returns>
        public string LoadAssetText(string assetFilename)
        {
            using (var stream = this.OpenAssetStream(assetFilename))
            {
                var streamReader = new StreamReader(stream);
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Opens asset stream; not implemented, throws exception
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>nothing, always throws exception</returns>
        public Stream OpenAssetStream(string assetFilename)
        {
            string filename = Path.Combine(
                Path.GetDirectoryName(this.GetType().Assembly.Location),
                "Assets",
                assetFilename);

            return new FileStream(filename, FileMode.Open);
        }

        /// <summary>
        /// Sets app theme to use for platform
        /// </summary>
        /// <param name="requestedTheme">requested theme</param>
        public void SetPlatformTheme(OSAppTheme requestedTheme)
        {
            // nothing to do
        }

        /// <summary>
        /// Translates the compass' magnetic north heading (e.g. from Xamarin.Essentials.Compass
        /// API) to true north. For unit tests, translates the value with a fixed declination.
        /// </summary>
        /// <param name="headingMagneticNorthInDegrees">magnetic north heading</param>
        /// <param name="latitudeInDegrees">latitude of current position</param>
        /// <param name="longitudeInDegrees">longitude of current position</param>
        /// <param name="altitudeInMeter">altitude of current position</param>
        /// <param name="headingTrueNorthInDegrees">true north heading</param>
        /// <returns>true when tralslating was successful, false when not available</returns>
        public bool TranslateCompassMagneticNorthToTrueNorth(
            int headingMagneticNorthInDegrees,
            double latitudeInDegrees,
            double longitudeInDegrees,
            double altitudeInMeter,
            out int headingTrueNorthInDegrees)
        {
            // Note: This is only for unit testing purposes!
            headingTrueNorthInDegrees = headingMagneticNorthInDegrees + 4;
            return true;
        }
    }
}
