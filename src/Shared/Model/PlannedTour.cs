using System;
using System.Collections.Generic;
using WhereToFly.Geo.Model;

namespace WhereToFly.Shared.Model
{
    /// <summary>
    /// Planned tour
    /// </summary>
    public class PlannedTour
    {
        /// <summary>
        /// All tour entires as list
        /// </summary>
        public List<PlannedTourEntry> TourEntriesList { get; set; } = new List<PlannedTourEntry>();

        /// <summary>
        /// List of map points for the whole tour
        /// </summary>
        public List<MapPoint> MapPointList { get; set; } = new List<MapPoint>();

        /// <summary>
        /// Total duration for all tour entires
        /// </summary>
        public TimeSpan TotalDuration { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Description for the complete tour
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
