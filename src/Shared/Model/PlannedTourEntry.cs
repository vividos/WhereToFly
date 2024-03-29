﻿using System;

namespace WhereToFly.Shared.Model
{
    /// <summary>
    /// Single tour entry in a planned tour
    /// </summary>
    public record PlannedTourEntry
    {
        /// <summary>
        /// Creates a new planned tour entry
        /// </summary>
        /// <param name="startWaypointId">start waypoint ID</param>
        /// <param name="endWaypointId">end waypoint ID</param>
        public PlannedTourEntry(string startWaypointId, string endWaypointId)
        {
            this.StartWaypointId = startWaypointId;
            this.EndWaypointId = endWaypointId;
        }

        /// <summary>
        /// Start waypoint ID
        /// </summary>
        public string StartWaypointId { get; set; }

        /// <summary>
        /// End waypoint ID
        /// </summary>
        public string EndWaypointId { get; set; }

        /// <summary>
        /// Distance in km for this entry
        /// </summary>
        public double DistanceInKm { get; set; } = 0.0;

        /// <summary>
        /// Duration for this entry
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Index of track points in the PlannedTour object, MapPointList list
        /// </summary>
        public int TrackStartIndex { get; set; } = 0;
    }
}
