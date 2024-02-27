using WhereToFly.Geo.Model;

namespace WhereToFly.App
{
    /// <summary>
    /// Extension methods for the app
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Converts a geo location to <see cref="MapPoint"/>
        /// </summary>
        /// <param name="location">geo location object; must not be null</param>
        /// <returns>map point</returns>
        public static MapPoint ToMapPoint(this Xamarin.Essentials.Location location)
        {
            return new MapPoint(
                location.Latitude,
                location.Longitude,
                location.Altitude);
        }
    }
}
