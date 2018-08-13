using System.Collections.Generic;

namespace WhereToFly.App.Geo
{
    /// <summary>
    /// A single track consisting of track points
    /// </summary>
    public class Track
    {
        /// <summary>
        /// ID for the track; e.g. generated at import time
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Track name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of track points
        /// </summary>
        public List<TrackPoint> TrackPoints { get; set; }

        /// <summary>
        /// Creates a new and empty track object
        /// </summary>
        public Track()
        {
            this.Name = string.Empty;
            this.TrackPoints = new List<TrackPoint>();
        }
    }
}
