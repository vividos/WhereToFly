using System;

namespace WhereToFly.App.Geo.Spatial
{
    /// <summary>
    /// Position data
    /// </summary>
    public class Position
    {
        /// <summary>
        /// Location of pilot in WGS84 coordinates
        /// </summary>
        public LatLongAlt Location { get; set; }

        /// <summary>
        /// Timestamp of location
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }
}
