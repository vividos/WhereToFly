using System;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo
{
    /// <summary>
    /// Extension methods for MapPoint class.
    /// Calculation formulas are used from Aviation Formulary, from Ed Williams:
    /// https://www.edwilliams.org/avform.htm
    /// </summary>
    public static class MapPointExtensionMethods
    {
        /// <summary>
        /// Calculates distance to other lat/long pair, and returns it.
        /// Uses the more exact formula for small distances.
        /// </summary>
        /// <param name="me">the map point object to calculate distance from</param>
        /// <param name="other">other lat/long value</param>
        /// <returns>distance in menter</returns>
        public static double DistanceTo(this MapPoint me, MapPoint other)
        {
            var lat1 = me.Latitude.ToRadians();
            var lat2 = other.Latitude.ToRadians();
            double deltaLat12 = (me.Latitude - other.Latitude).ToRadians();
            double deltaLong12 = (me.Longitude - other.Longitude).ToRadians();

            double distanceRadians =
                2 * Math.Asin(
                    Math.Sqrt(
                        Math.Pow(Math.Sin(deltaLat12 / 2), 2) +
                        (Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLong12 / 2), 2))));

            return distanceRadians * Constants.EarthRadiusInMeter;
        }

        /// <summary>
        /// Calculates the course to use to travel to other lat/long pair. The course angle is
        /// expressed in degrees clockwise from North, in the range [0,360).
        /// </summary>
        /// <param name="me">the map point object to calculate course from</param>
        /// <param name="other">other lat/long value</param>
        /// <returns>course in degrees</returns>
        public static double CourseTo(this MapPoint me, MapPoint other)
        {
            double trueCourse;

            var lat1 = me.Latitude.ToRadians();
            if (Math.Abs(Math.Cos(lat1)) < 1e-6)
            {
                // starting from N or S pole
                trueCourse = lat1 > 0 ? Math.PI : 0.0;
            }
            else
            {
                // Note: this delta is reversed from the Aviary formula, in order to get angles
                // clockwise from North
                double deltaLong21 = (other.Longitude - me.Longitude).ToRadians();
                var lat2 = other.Latitude.ToRadians();

                trueCourse = Math.Atan2(
                    Math.Sin(deltaLong21) * Math.Cos(lat2),
                    (Math.Cos(lat1) * Math.Sin(lat2)) - (Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLong21)))
                    % (2 * Math.PI);

                if (trueCourse < 0.0)
                {
                    trueCourse += 2 * Math.PI;
                }
            }

            return trueCourse.ToDegrees();
        }

        /// <summary>
        /// Offsets the current latitude/longitude values by given distances and returns a new
        /// value.
        /// </summary>
        /// <param name="me">the map point object to offset</param>
        /// <param name="northDistanceInMeter">distance in north direction, in meters</param>
        /// <param name="eastDistanceInMeter">distance in east direction, in meters</param>
        /// <param name="heightDistanceInMeters">height distance, in meters</param>
        /// <returns>new point</returns>
        public static MapPoint Offset(this MapPoint me, double northDistanceInMeter, double eastDistanceInMeter, double heightDistanceInMeters)
        {
            double distanceInMeter = Math.Sqrt(
                (northDistanceInMeter * northDistanceInMeter) +
                (eastDistanceInMeter * eastDistanceInMeter));

            return me.PolarOffset(
                distanceInMeter,
                -Math.Atan2(northDistanceInMeter, eastDistanceInMeter).ToDegrees(),
                heightDistanceInMeters);
        }

        /// <summary>
        /// Offsets the current latitude/longitude values by polar distance and bearing in degrees
        /// and returns a new value. The bearing angle is expressed in degrees clockwise from
        /// North, in the range [0,360).
        /// </summary>
        /// <param name="me">the map point object to offset</param>
        /// <param name="distanceInMeter">distance to move, in meter</param>
        /// <param name="bearingInDegrees">
        /// bearing in degress, with 0 degrees being North, 90 being East, 180 being South and 270
        /// being West.
        /// </param>
        /// <param name="heightDistanceInMeters">height distance, in meters</param>
        /// <returns>new point</returns>
        public static MapPoint PolarOffset(
            this MapPoint me,
            double distanceInMeter,
            double bearingInDegrees,
            double heightDistanceInMeters)
        {
            double lat1 = me.Latitude.ToRadians();
            double long1 = me.Longitude.ToRadians();
            double angularDistanceRadians = distanceInMeter / Constants.EarthRadiusInMeter;
            double trueCourseRadians = -bearingInDegrees.ToRadians();

            double newLat = Math.Asin(
                (Math.Sin(lat1) * Math.Cos(angularDistanceRadians)) +
                (Math.Cos(lat1) * Math.Sin(angularDistanceRadians) * Math.Cos(trueCourseRadians)));

            double deltaLong = Math.Atan2(
                Math.Sin(trueCourseRadians) * Math.Sin(angularDistanceRadians) * Math.Cos(lat1),
                Math.Cos(angularDistanceRadians) - (Math.Sin(lat1) * Math.Sin(newLat)));

            double newLong = ((long1 - deltaLong + Math.PI) % (2 * Math.PI)) - Math.PI;

            return new MapPoint(
                newLat.ToDegrees(),
                newLong.ToDegrees(),
                me.Altitude.GetValueOrDefault(0.0) + heightDistanceInMeters);
        }
    }
}
