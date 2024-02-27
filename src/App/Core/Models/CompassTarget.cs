using WhereToFly.Geo.Model;

namespace WhereToFly.App.Models
{
    /// <summary>
    /// Specifies the current compass target, either a fixed location or a direction angle from
    /// current location. Note that target location overrides the target direction, if both are
    /// set.
    /// </summary>
    public record CompassTarget
    {
        /// <summary>
        /// Title of compass target
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Target location map point
        /// </summary>
        public MapPoint? TargetLocation { get; set; }

        /// <summary>
        /// Target direction angle, in degrees
        /// </summary>
        public int? TargetDirection { get; set; }
    }
}
