namespace WhereToFly.App
{
    /// <summary>
    /// Compass geographic services
    /// </summary>
    public class CompassGeoServices
    {
        /// <summary>
        /// Translates the compass' magnetic north heading (e.g. from the Essentials' Compass
        /// API) to true north.
        /// On Android this is done using the GeomagneticField class that returns the magnetic
        /// declination on the given coordinates.
        /// On Windows, there's no such API.
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
#if ANDROID
            long millisecondsSinceEpoch = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var geomagneticField = new global::Android.Hardware.GeomagneticField(
                (float)latitudeInDegrees,
                (float)longitudeInDegrees,
                (float)altitudeInMeter,
                millisecondsSinceEpoch);

            headingTrueNorthInDegrees =
                (int)(headingMagneticNorthInDegrees + geomagneticField.Declination);

            return true;
#elif WINDOWS
            // on Windows this isn't possible. Essentials' Compass could return true north
            // heading directly in the CompassData, but doesn't.
            headingTrueNorthInDegrees = 0;
            return false;
#else
            // for unit tests
            headingTrueNorthInDegrees = headingMagneticNorthInDegrees + 4;
            return true;
#endif
        }
    }
}
