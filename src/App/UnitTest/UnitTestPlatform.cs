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
