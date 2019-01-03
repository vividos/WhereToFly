namespace WhereToFly.WebApi.Logic.TourPlanning
{
    /// <summary>
    /// Track info that represents a node in the tour graph
    /// </summary>
    internal class WaypointInfo
    {
        /// <summary>
        /// Waypoint ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Description of waypoint
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Returns a displayable text for the waypoint info
        /// </summary>
        /// <returns>displayable text</returns>
        public override string ToString() => $"Id={this.Id}";
    }
}
