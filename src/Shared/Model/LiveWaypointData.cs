using System;

namespace WhereToFly.Shared.Model
{
    /// <summary>
    /// Data for a Live Waypoint
    /// </summary>
    public class LiveWaypointData
    {
        /// <summary>
        ///  Live Waypoint ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Time stamp of last update to live waypoint data
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }

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
        public int Altitude { get; set; }

        /// <summary>
        /// Name of live waypoint
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Detailed description, in MarkDown format
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Details link, e.g. to external website
        /// </summary>
        public string DetailsLink { get; set; }

        /// <summary>
        /// Date where next request for this specific live waypoint may result in new data
        /// </summary>
        public DateTimeOffset NextRequestDate { get; }
    }
}
