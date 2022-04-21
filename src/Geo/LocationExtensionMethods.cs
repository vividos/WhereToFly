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
                MapPoint mapLocation = location.MapLocation;
                minLocation.Latitude = Math.Min(mapLocation.Latitude, minLocation.Latitude);
                maxLocation.Latitude = Math.Max(mapLocation.Latitude, maxLocation.Latitude);
                minLocation.Longitude = Math.Min(mapLocation.Longitude, minLocation.Longitude);
                maxLocation.Longitude = Math.Max(mapLocation.Longitude, maxLocation.Longitude);
                minLocation.Altitude = Math.Min(
                    mapLocation.Altitude ?? defaultMinAltitudeValue,
                    minLocation.Altitude ?? defaultMinAltitudeValue);
                maxLocation.Altitude = Math.Max(
                    mapLocation.Altitude ?? defaultMaxAltitudeValue,
                    maxLocation.Altitude ?? defaultMaxAltitudeValue);
            }

            if (!minLocation.Altitude.HasValue ||
                Math.Abs(minLocation.Altitude.Value - defaultMinAltitudeValue) < 1e-6)
            {
                minLocation.Altitude = 0.0;
            }

            if (!maxLocation.Altitude.HasValue ||
                Math.Abs(maxLocation.Altitude.Value - defaultMaxAltitudeValue) < 1e-6)
            {
                maxLocation.Altitude = 0.0;
            }
        }
    }
}
