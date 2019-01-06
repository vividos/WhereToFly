using System;
using WhereToFly.Shared.Model;

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
        public MapPoint Location { get; set; }

        /// <summary>
        /// Timestamp of location
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }
}
