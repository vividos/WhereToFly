using System;
using System.Text.Json.Serialization;

namespace WhereToFly.Geo.Model
{
    /// <summary>
    /// Single track point in a track
    /// </summary>
    [JsonConverter(typeof(Serializers.TrackPointConverter))]
    public class TrackPoint
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
        /// Altitude, in meters; optional
        /// </summary>
        public double? Altitude { get; set; }

        /// <summary>
        /// Heading, in degrees; optional
        /// </summary>
        public int? Heading { get; set; }

        /// <summary>
        /// Date and time of track point
        /// </summary>
        public DateTimeOffset? Time { get; set; }

        /// <summary>
        /// Creates a new track point from given data
        /// </summary>
        /// <param name="latitude">latitude value</param>
        /// <param name="longitude">longitude value</param>
        /// <param name="altitude">altitude value; optional</param>
        /// <param name="heading">heading; optional</param>
        public TrackPoint(double latitude, double longitude, double? altitude, int? heading)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
            this.Heading = heading;
        }
    }
}
