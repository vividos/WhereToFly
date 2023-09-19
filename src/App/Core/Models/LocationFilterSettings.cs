using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.Models
{
    /// <summary>
    /// Location filter settings
    /// </summary>
    public record LocationFilterSettings
    {
        /// <summary>
        /// Last used filter text
        /// </summary>
        public string FilterText { get; set; } = string.Empty;

        /// <summary>
        /// Takeoff directions to filter
        /// </summary>
        public TakeoffDirections FilterTakeoffDirections { get; set; } = TakeoffDirections.All;

        /// <summary>
        /// Filter settings that determines if non-takeoff locations should also be shown
        /// </summary>
        public bool ShowNonTakeoffLocations { get; set; } = true;

        #region object overridables implementation

        /// <summary>
        /// Returns a displayable string
        /// </summary>
        /// <returns>displayable string</returns>
        public override string ToString()
        {
            return $"Text={this.FilterText}, TakeoffDirections={this.FilterTakeoffDirections}, ShowNonTakeoffLocations={this.ShowNonTakeoffLocations}";
        }
        #endregion
    }
}
