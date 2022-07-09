using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo.Airspace;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for selecting airspace classes to import.
    /// </summary>
    public partial class SelectAirspaceClassPopupPage : BasePopupPage
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly SelectAirspaceClassPopupViewModel viewModel;

        /// <summary>
        /// Task completion source to report back selected airspace classes
        /// </summary>
        private TaskCompletionSource<ISet<AirspaceClass>> tcs;

        /// <summary>
        /// Creates a new popup page to select airspace classes
        /// </summary>
        /// <param name="airspaceClassesList">airspace classes to choose from</param>
        public SelectAirspaceClassPopupPage(IEnumerable<AirspaceClass> airspaceClassesList)
        {
            this.BindingContext = this.viewModel =
                new SelectAirspaceClassPopupViewModel(airspaceClassesList);

            this.CloseWhenBackgroundIsClicked = true;

            this.InitializeComponent();
        }

        /// <summary>
        /// Shows "select airspace classes" popup page and lets the user select airspace classes.
        /// </summary>
        /// <param name="airspaceClassesList">list of airspace classes</param>
        /// <returns>
        /// set of all selected airspace classes, or null when user canceled the popup dialog
        /// </returns>
        public static async Task<ISet<AirspaceClass>> ShowAsync(IEnumerable<AirspaceClass> airspaceClassesList)
        {
            var popupPage = new SelectAirspaceClassPopupPage(airspaceClassesList)
            {
                tcs = new TaskCompletionSource<ISet<AirspaceClass>>(),
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
        /// Called when user clicked on the "Filter airspaces" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnClickedFilterAirspacesButton(object sender, EventArgs args)
        {
            await this.Navigation.PopPopupAsync();

            var airspaceClassesSet = this.viewModel.GetSelectedAirspaceClasses();

            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(airspaceClassesSet);
            }
        }
    }
}
