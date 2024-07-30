using System.Collections.Generic;
using WhereToFly.Geo.Model;

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
        public List<string> WaypointIdList { get; set; } = [];

        /// <summary>
        /// List of locations of waypoint IDs not known to the backend
        /// </summary>
        public List<Location> WaypointLocationList { get; set; } = [];
    }
}
