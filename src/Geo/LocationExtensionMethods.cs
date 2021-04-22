using System;
using System.Collections.Generic;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo
{
    /// <summary>
    /// Extension methods for Location objects
    /// </summary>
    public static class LocationExtensionMethods
    {
        /// <summary>
        /// Gets bounding rectangle of a list of locations
        /// </summary>
        /// <param name="locationList">location list to use</param>
        /// <param name="minLocation">minimum location map point</param>
        /// <param name="maxLocation">maximum location map point</param>
        public static void GetBoundingRectangle(
            this IEnumerable<Location> locationList,
            out MapPoint minLocation,
            out MapPoint maxLocation)
        {
            const double defaultMinAltitudeValue = 10000.0;
            const double defaultMaxAltitudeValue = -10000.0;

            minLocation = new MapPoint(90.0, 360.0, defaultMinAltitudeValue);
            maxLocation = new MapPoint(-90.0, -360.0, defaultMaxAltitudeValue);
            foreach (var location in locationList)
            {
                minLocation.Latitude = Math.Min(location.MapLocation.Latitude, minLocation.Latitude);
                maxLocation.Latitude = Math.Max(location.MapLocation.Latitude, maxLocation.Latitude);
                minLocation.Longitude = Math.Min(location.MapLocation.Longitude, minLocation.Longitude);
                maxLocation.Longitude = Math.Max(location.MapLocation.Longitude, maxLocation.Longitude);
                minLocation.Altitude = Math.Min(location.MapLocation.Altitude ?? defaultMinAltitudeValue, minLocation.Altitude.Value);
                maxLocation.Altitude = Math.Max(location.MapLocation.Altitude ?? defaultMaxAltitudeValue, maxLocation.Altitude.Value);
            }

            if (Math.Abs(minLocation.Altitude.Value - defaultMinAltitudeValue) < 1e-6)
            {
                minLocation.Altitude = 0.0;
            }

            if (Math.Abs(maxLocation.Altitude.Value - defaultMaxAltitudeValue) < 1e-6)
            {
                maxLocation.Altitude = 0.0;
            }
        }
    }
}
