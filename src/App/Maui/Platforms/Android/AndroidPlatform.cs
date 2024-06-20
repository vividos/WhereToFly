[assembly: Dependency(typeof(WhereToFly.App.AndroidPlatform))]

namespace WhereToFly.App
{
    /// <summary>
    /// Platform specific functions
    /// </summary>
    public class AndroidPlatform : IPlatform
    {
        /// <summary>
        /// Base path to use in WebView control, for Android
        /// </summary>
        public string WebViewBasePath => "file:///android_asset/";

        /// <summary>
        /// Translates the compass' magnetic north heading (e.g. from Xamarin.Essentials.Compass
        /// API) to true north. On Android this is done using the GeomagneticField class that
        /// returns the magnetic declination on the given coordinates
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
            long millisecondsSinceEpoch = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var geomagneticField = new global::Android.Hardware.GeomagneticField(
                (float)latitudeInDegrees,
                (float)longitudeInDegrees,
                (float)altitudeInMeter,
                millisecondsSinceEpoch);

            headingTrueNorthInDegrees =
                (int)(headingMagneticNorthInDegrees + geomagneticField.Declination);

            return true;
        }
    }
}
