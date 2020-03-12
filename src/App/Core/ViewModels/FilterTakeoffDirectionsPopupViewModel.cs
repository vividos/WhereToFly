using WhereToFly.App.Model;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the "filter by takeoff directions" popup page
    /// </summary>
    public class FilterTakeoffDirectionsPopupViewModel
    {
        #region Binding properties
        /// <summary>
        /// Takeoff directions flag to use for filtering
        /// </summary>
        public TakeoffDirections TakeoffDirections { get; set; }

        /// <summary>
        /// Indicates if other locations without takeoff directions should also be shown
        /// </summary>
        public bool AlsoShowOtherLocations { get; set; }
        #endregion

        /// <summary>
        /// Creates a new view model for the "filter by takeoff directions" popup page
        /// </summary>
        /// <param name="takeoffDirections">takeoff directions flag to use</param>
        /// <param name="alsoShowOtherLocations">flag if other locations should also be shown</param>
        public FilterTakeoffDirectionsPopupViewModel(TakeoffDirections takeoffDirections, bool alsoShowOtherLocations)
        {
            this.TakeoffDirections = takeoffDirections;
            this.AlsoShowOtherLocations = alsoShowOtherLocations;
        }
    }
}
