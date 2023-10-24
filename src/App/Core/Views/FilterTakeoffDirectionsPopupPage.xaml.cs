using System;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.ViewModels;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for specifying a filter based on takeoff directions for the location list.
    /// </summary>
    public partial class FilterTakeoffDirectionsPopupPage : BasePopupPage<LocationFilterSettings>
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly FilterTakeoffDirectionsPopupViewModel viewModel;

        /// <summary>
        /// Creates a new popup page to edit filter settings
        /// </summary>
        /// <param name="filterSettings">filter settings to edit</param>
        public FilterTakeoffDirectionsPopupPage(LocationFilterSettings filterSettings)
        {
            this.BindingContext = this.viewModel =
                new FilterTakeoffDirectionsPopupViewModel(filterSettings);

            this.InitializeComponent();
        }

        /// <summary>
        /// Called when user clicked on the "Filter" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnClickedFilterButton(object sender, EventArgs args)
        {
            this.SetResult(this.viewModel.FilterSettings);
        }
    }
}
