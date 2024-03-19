using QuickGraph;
using System;
using System.Collections.Generic;
using WhereToFly.Geo.Model;

namespace WhereToFly.WebApi.Logic.TourPlanning
{
    /// <summary>
    /// Track info that represents an edge in the tour graph
    /// </summary>
    internal class TrackInfo : IEdge<WaypointInfo>
    {
        /// <summary>
        /// Description for this track
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Duration of track for normal hikers
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// List of map points for this track
        /// </summary>
        public List<MapPoint> MapPointList { get; set; } = new List<MapPoint>();

        #region IEdge implementation

        /// <summary>
        /// Edge source; the waypoint where the track starts
        /// </summary>
        public WaypointInfo Source { get; set; }

        /// <summary>
        /// Edge target; the waypoint where the track ends
        /// </summary>
        public WaypointInfo Target { get; set; }
        #endregion

        /// <summary>
        /// Creates a new track info object
        /// </summary>
        /// <param name="source">edge source waypoint</param>
        /// <param name="target">edge target waypoint</param>
        /// <param name="description">track description</param>
        /// <param name="duration">track duration</param>
        public TrackInfo(
            WaypointInfo source,
            WaypointInfo target,
            string description,
            TimeSpan duration)
        {
            this.Source = source;
            this.Target = target;
            this.Description = description;
            this.Duration = duration;
        }

        /// <summary>
        /// Returns a displayable text for the track info
        /// </summary>
        /// <returns>displayable text</returns>
        public override string ToString() => $"Source={this.Source.Id}, Target={this.Target.Id}";
    }
}
