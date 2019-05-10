using System;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// Event arguments for the event when live waypoint data has been updated
    /// </summary>
    public class LiveWaypointUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Updated live waypoint data
        /// </summary>
        public LiveWaypointData Data { get; set; }
    }
}
