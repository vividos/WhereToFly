using System.Collections.Generic;

namespace WhereToFly.Shared.Model
{
    /// <summary>
    /// Parameters for planning a tour
    /// </summary>
    public class PlanTourParameters
    {
        /// <summary>
        /// List of waypoint IDs of tour locations to visit
        /// </summary>
        public List<string> WaypointIdList { get; set; } = new List<string>();
    }
}
