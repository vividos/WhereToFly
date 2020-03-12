using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Model;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for specifying a filter based on takeoff directions for the location list.
    /// </summary>
    public partial class FilterTakeoffDirectionsPopupPage : PopupPage
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly FilterTakeoffDirectionsPopupViewModel viewModel;

        /// <summary>
        /// Task completion source to report back filter parameters
        /// </summary>
        private TaskCompletionSource<Tuple<TakeoffDirections, bool>> tcs;

        /// <summary>
        /// Creates a new popup page to edit track properties
        /// </summary>
        /// <param name="track">track to edit</param>

        public FilterTakeoffDirectionsPopupPage(TakeoffDirections takeoffDirections, bool alsoShowOtherLocations)
        {
            this.CloseWhenBackgroundIsClicked = true;

            this.BindingContext = this.viewModel =
                new FilterTakeoffDirectionsPopupViewModel(takeoffDirections, alsoShowOtherLocations);

            this.InitializeComponent();
        }

        /// <summary>
        /// Shows "filter by takeoff directions" popup page and lets the user set the filter
        /// criteria.
        /// </summary>
        /// <param name="track">track to edit</param>
        /// <returns>entered text, or null when user canceled the popup dialog</returns>
        public static async Task<Tuple<TakeoffDirections, bool>> ShowAsync(TakeoffDirections takeoffDirections, bool alsoShowOtherLocations)
        {
            var popupPage = new FilterTakeoffDirectionsPopupPage(takeoffDirections, alsoShowOtherLocations)
            {
                tcs = new TaskCompletionSource<Tuple<TakeoffDirections, bool>>()
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
                this.tcs.SetResult(new Tuple<TakeoffDirections, bool>(
                    this.viewModel.TakeoffDirections,
                    this.viewModel.AlsoShowOtherLocations));
            }

            await this.Navigation.PopPopupAsync();
        }
    }
}
