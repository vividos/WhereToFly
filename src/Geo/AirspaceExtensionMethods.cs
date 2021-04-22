using WhereToFly.Geo.Airspace;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo
{
    /// <summary>
    /// Extension methods for Airspace data types
    /// </summary>
    public static class AirspaceExtensionMethods
    {
        /// <summary>
        /// Converts a Coord object and height to a MapPoint
        /// </summary>
        /// <param name="me">coord object</param>
        /// <param name="height">height value</param>
        /// <returns>map point</returns>
        public static MapPoint ToMapPoint(this Coord me, double height)
        {
            return new MapPoint(me.Latitude, me.Longitude, height);
        }
    }
}
