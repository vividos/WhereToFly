using WhereToFly.App.Core.Models;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the "filter by takeoff directions" popup page
    /// </summary>
    public class FilterTakeoffDirectionsPopupViewModel
    {
        /// <summary>
        /// Filter settings to edit
        /// </summary>
        public LocationFilterSettings FilterSettings { get; private set; }

        #region Binding properties
        /// <summary>
        /// Takeoff directions flag to use for filtering
        /// </summary>
        public TakeoffDirections TakeoffDirections
        {
            get => this.FilterSettings.FilterTakeoffDirections;
            set => this.FilterSettings.FilterTakeoffDirections = value;
        }

        /// <summary>
        /// Indicates if other locations without takeoff directions should also be shown
        /// </summary>
        public bool AlsoShowOtherLocations
        {
            get => this.FilterSettings.ShowNonTakeoffLocations;
            set => this.FilterSettings.ShowNonTakeoffLocations = value;
        }
        #endregion

        /// <summary>
        /// Creates a new view model for the "filter by takeoff directions" popup page
        /// </summary>
        /// <param name="filterSettings">filter settings to edit</param>
        public FilterTakeoffDirectionsPopupViewModel(LocationFilterSettings filterSettings)
        {
            this.FilterSettings = filterSettings;
        }
    }
}
