using System;

namespace WhereToFly.Shared.Model
{
    /// <summary>
    /// Data for a Live Track
    /// </summary>
    public class LiveTrackData
    {
        /// <summary>
        /// Live Track ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Name of live waypoint
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Detailed description, in MarkDown format
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Track starting point
        /// </summary>
        public DateTimeOffset TrackStart { get; set; }

        /// <summary>
        /// Live tracking track point
        /// </summary>
        public struct LiveTrackPoint
        {
            /// <summary>
            /// Offset of track point to track start
            /// </summary>
            public double Offset { get; set; }

            /// <summary>
            /// Latitude value, positive values to the north
            /// </summary>
            public double Latitude { get; set; }

            /// <summary>
            /// Longitude value, positive values to the east
            /// </summary>
            public double Longitude { get; set; }

            /// <summary>
            /// Altitude value, if available; 0 means "clamp to ground"
            /// </summary>
            public double Altitude { get; set; }
        }

        /// <summary>
        /// List of all track points
        /// </summary>
        public LiveTrackPoint[] TrackPoints { get; set; } = Array.Empty<LiveTrackPoint>();
    }
}
