using System;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// Event arguments for the event when live data has been updated
    /// </summary>
    public class LiveDataUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Updated live waypoint data; may be null
        /// </summary>
        public LiveWaypointData WaypointData { get; set; }

        /// <summary>
        /// Updated live track data; may be null
        /// </summary>
        public LiveTrackData TrackData { get; set; }
    }
}
