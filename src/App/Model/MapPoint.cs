using System;
using System.Globalization;

namespace WhereToFly.App.Model
{
    /// <summary>
    /// A point on a map, in WGS84 decimal coordinates. Negative values are
    /// left of the GMT line and below the equator.
    /// </summary>
    public sealed class MapPoint : IEquatable<MapPoint>
    {
        /// <summary>
        /// Creates a new map point
        /// </summary>
        /// <param name="latitude">latitude in decimal degrees</param>
        /// <param name="longitude">longitude in decimal degrees</param>
        /// <param name="altitude">altitude in meters; optional</param>
        public MapPoint(double latitude, double longitude, double? altitude = null)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
        }

        /// <summary>
        /// Latitude, from north (+90.0) to south (-90.0), 0.0 at equator line, e.g. 48.137155
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude, from west to east, 0.0 at Greenwich line; e.g. 11.575416
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Altitude above mean sea level, in meters; optional
        /// </summary>
        public double? Altitude { get; set; }

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

        #region IEquatable implementation
        /// <summary>
        /// Compares this map point to another map point and returns if they are equal
        /// </summary>
        /// <param name="other">other map point</param>
        /// <returns>true when equal, false when not</returns>
        public bool Equals(MapPoint other)
        {
            if (other == null)
            {
                return false;
            }

            bool altitudeIsEqual =
                (this.Altitude == null && other.Altitude == null) ||
                (this.Altitude.HasValue == other.Altitude.HasValue && Math.Abs(this.Altitude.Value - other.Altitude.Value) < 1e-2);

            return Math.Abs(this.Latitude - other.Latitude) < 1e-6 &&
                Math.Abs(this.Longitude - other.Longitude) < 1e-6 &&
                altitudeIsEqual;
        }
        #endregion

        #region object overridables implementation
        /// <summary>
        /// Compares this map point to another object and returns if they are equal
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>true when equal, false when not</returns>
        public override bool Equals(object obj)
        {
            var other = obj as MapPoint;

            if (other == null)
            {
                return false;
            }

            return this.Equals(other);
        }

        /// <summary>
        /// Calculates hash code for map point
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = 487;
            hashCode = (hashCode * 31) + this.Latitude.GetHashCode();
            hashCode = (hashCode * 31) + this.Longitude.GetHashCode();
            hashCode = (hashCode * 31) + this.Altitude.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Returns a printable representation of this object
        /// </summary>
        /// <returns>printable text</returns>
        public override string ToString()
        {
            if (!this.Valid)
            {
                return "invalid";
            }

            return string.Format(
                "Lat={0}, Long={1}, Alt={2}",
                this.Latitude.ToString("F6", CultureInfo.InvariantCulture),
                this.Longitude.ToString("F6", CultureInfo.InvariantCulture),
                this.Altitude.HasValue ? this.Altitude.Value.ToString("F2", CultureInfo.InvariantCulture) : "N/A");
        }
        #endregion
    }
}
