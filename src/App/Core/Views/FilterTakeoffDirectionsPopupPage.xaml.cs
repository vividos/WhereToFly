using Rg.Plugins.Popup.Extensions;
using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.ViewModels;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for specifying a filter based on takeoff directions for the location list.
    /// </summary>
    public partial class FilterTakeoffDirectionsPopupPage : BasePopupPage
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly FilterTakeoffDirectionsPopupViewModel viewModel;

        /// <summary>
        /// Task completion source to report back filter settings
        /// </summary>
        private TaskCompletionSource<LocationFilterSettings> tcs;

        /// <summary>
        /// Creates a new popup page to edit filter settings
        /// </summary>
        /// <param name="filterSettings">filter settings to edit</param>
        public FilterTakeoffDirectionsPopupPage(LocationFilterSettings filterSettings)
        {
            this.CloseWhenBackgroundIsClicked = true;

            this.BindingContext = this.viewModel =
                new FilterTakeoffDirectionsPopupViewModel(filterSettings);

            this.InitializeComponent();
        }

        /// <summary>
        /// Shows "filter by takeoff directions" popup page and lets the user set the filter
        /// criteria.
        /// </summary>
        /// <param name="filterSettings">filter settings to edit</param>
        /// <returns>filter settings, or null when user canceled the popup dialog</returns>
        public static async Task<LocationFilterSettings> ShowAsync(LocationFilterSettings filterSettings)
        {
            var popupPage = new FilterTakeoffDirectionsPopupPage(filterSettings)
            {
                tcs = new TaskCompletionSource<LocationFilterSettings>(),
            };

            await popupPage.Navigation.PushPopupAsync(popupPage);

            return await popupPage.tcs.Task;
        }

        /// <summary>
        /// Called when user clicked on the background, dismissing the popup page.
        /// </summary>
        /// <returns>whatever the base class returns</returns>
        protected override bool OnBackgroundClicked()
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(null);
            }

            return base.OnBackgroundClicked();
        }

        /// <summary>
        /// Called when user naviaged back with the back button, dismissing the popup page.
        /// </summary>
        /// <returns>whatever the base class returns</returns>
        protected override bool OnBackButtonPressed()
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(null);
            }

            return base.OnBackButtonPressed();
        }

        /// <summary>
        /// Called when user clicked on the "Filter" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnClickedFilterButton(object sender, EventArgs args)
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(this.viewModel.FilterSettings);
            }

            await this.Navigation.PopPopupAsync();
        }
    }
}
