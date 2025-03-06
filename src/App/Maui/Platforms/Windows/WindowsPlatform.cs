[assembly: Dependency(typeof(WhereToFly.App.WindowsPlatform))]

namespace WhereToFly.App
{
    /// <summary>
    /// Platform specific functions
    /// </summary>
    public class WindowsPlatform : IPlatform
    {
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
        public bool TranslateCompassMagneticNorthToTrueNorth(
            int headingMagneticNorthInDegrees,
            double latitudeInDegrees,
            double longitudeInDegrees,
            double altitudeInMeter,
            out int headingTrueNorthInDegrees)
        {
            // on Windows this isn't possible. Essentials' Compass could return true north
            // heading directly in the CompassData, but doesn't.
            headingTrueNorthInDegrees = 0;
            return false;
        }
    }
}
