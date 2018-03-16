using System;
using System.Globalization;

namespace WhereToFly.Geo.Spatial
{
    /// <summary>
    /// Latitude, longitude and altitude values in WGS84 coordinate system
    /// </summary>
    public class LatLongAlt
    {
        /// <summary>
        /// Latitude, from north (+90.0) to south (-90.0), 0.0 at equator line, e.g. 48.137155
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude, from west to east, 0.0 at Greenwich line; e.g. 11.575416
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Altitude value, in meters; optional
        /// </summary>
        public double? Altitude { get; private set; }

        /// <summary>
        /// Creates a new LatLongAlt object
        /// </summary>
        /// <param name="latitude">latitude in decimal degrees</param>
        /// <param name="longitude">longitude in decimal degrees</param>
        /// <param name="altitude">altitude value, in meters; optional</param>
        public LatLongAlt(double latitude, double longitude, double? altitude = null)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
        }

        /// <summary>
        /// Returns if map point is valid, e.g. when latitude and longitude are != 0
        /// </summary>
        public bool Valid
        {
            get
            {
                return
                    Math.Abs(this.Latitude) > double.Epsilon &&
                    Math.Abs(this.Longitude) > double.Epsilon;
            }
        }

        /// <summary>
        /// Calculates distance to other lat/long pair, and returns it.
        /// Uses the "great distance" formula.
        /// </summary>
        /// <param name="other">other lat/long value</param>
        /// <returns>distance in menter</returns>
        public double DistanceTo(LatLongAlt other)
        {
            var lat1 = this.Latitude.ToRadians();
            var lat2 = other.Latitude.ToRadians();
            var deltaLong12 = (this.Longitude - other.Longitude).ToRadians();

            double distanceArc =
                (Math.Sin(lat1) * Math.Sin(lat2)) +
                (Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(deltaLong12));

            double distanceRadians = Math.Acos(distanceArc).ToDegrees();

            return distanceRadians * Constants.EarthRadiusInMeter;
        }

        /// <summary>
        /// Offsets the current latitude/longitude values by given distances and returns a new
        /// value.
        /// </summary>
        /// <param name="northDistanceInMeter">distance in north direction, in meters</param>
        /// <param name="eastDistanceInMeter">distance in east direction, in meters</param>
        /// <param name="heightDistanceInMeters">height distance, in meters</param>
        /// <returns>new point</returns>
        public LatLongAlt Offset(double northDistanceInMeter, double eastDistanceInMeter, double heightDistanceInMeters)
        {
            double distanceInMeter = Math.Sqrt(
                (northDistanceInMeter * northDistanceInMeter) +
                (eastDistanceInMeter * eastDistanceInMeter));

            return this.PolarOffset(
                distanceInMeter,
                Math.Atan2(northDistanceInMeter, eastDistanceInMeter).ToDegrees(),
                heightDistanceInMeters);
        }

        /// <summary>
        /// Offsets the current latitude/longitude values by polar distance and bearing in degrees
        /// and returns a new value.
        /// </summary>
        /// <param name="distanceInMeter">distance to move, in meter</param>
        /// <param name="bearingInDegrees">
        /// bearing in degress, with 0 degrees being North, 90 being East, 180 being South and 270
        /// being West.
        /// </param>
        /// <param name="heightDistanceInMeters">height distance, in meters</param>
        /// <returns>new point</returns>
        public LatLongAlt PolarOffset(double distanceInMeter, double bearingInDegrees, double heightDistanceInMeters)
        {
            double lat1 = this.Latitude.ToRadians();
            double long1 = this.Longitude.ToRadians();
            double angularDistanceRadians = distanceInMeter / Constants.EarthRadiusInMeter;
            double trueCourseRadians = bearingInDegrees.ToRadians();

            double newLat = Math.Asin(
                (Math.Sin(lat1) * Math.Cos(angularDistanceRadians)) +
                (Math.Cos(lat1) * Math.Sin(angularDistanceRadians) * Math.Cos(trueCourseRadians)));

            double deltaLong = Math.Atan2(
                Math.Sin(trueCourseRadians) * Math.Sin(angularDistanceRadians) * Math.Cos(lat1),
                Math.Cos(angularDistanceRadians) - (Math.Sin(lat1) * Math.Sin(newLat)));

            double lon = ((long1 + deltaLong + Math.PI) % (2 * Math.PI)) - Math.PI;

            return new LatLongAlt(
                newLat.ToDegrees(),
                lon.ToDegrees(),
                this.Altitude.HasValue ? this.Altitude + heightDistanceInMeters : heightDistanceInMeters);
        }

        /// <summary>
        /// Returns a printable representation of this object
        /// </summary>
        /// <returns>printable text</returns>
        public override string ToString()
        {
            return string.Format(
                "Lat={0}, Long={1}, Alt={2}",
                this.Latitude.ToString("F6", CultureInfo.InvariantCulture),
                this.Longitude.ToString("F6", CultureInfo.InvariantCulture),
                this.Altitude.HasValue ? this.Altitude.Value.ToString("F2", CultureInfo.InvariantCulture) : "N/A");
        }
    }
}
