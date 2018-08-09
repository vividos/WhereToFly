using System;

namespace WhereToFly.App.Geo
{
    /// <summary>
    /// Single track point in a track
    /// </summary>
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
        public int? Altitude { get; set; }

        /// <summary>
        /// Heading, in degrees; optional
        /// </summary>
        public int? Heading { get; set; }

        /// <summary>
        /// Time offset from start of track
        /// </summary>
        public TimeSpan TimeOffset { get; set; }

        /// <summary>
        /// Creates a new track point from given data
        /// </summary>
        /// <param name="latitude">latitude value</param>
        /// <param name="longitude">longitude value</param>
        /// <param name="altitude">altitude value; optional</param>
        /// <param name="heading">heading; optional</param>
        public TrackPoint(double latitude, double longitude, int? altitude, int? heading)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
            this.Heading = heading;
            this.TimeOffset = TimeSpan.Zero;
        }
    }
}
