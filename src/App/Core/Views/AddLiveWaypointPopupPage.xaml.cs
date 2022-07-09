using Rg.Plugins.Popup.Extensions;
using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for adding a live waypoint and edit its properties.
    /// </summary>
    public partial class AddLiveWaypointPopupPage : BasePopupPage
    {
        /// <summary>
        /// Task completion source to report back if live waypoint should be added
        /// </summary>
        private TaskCompletionSource<bool> tcs;

        /// <summary>
        /// Creates a new popup page to edit live waypoint properties
        /// </summary>
        /// <param name="liveWaypoint">live waypoint to edit</param>
        public AddLiveWaypointPopupPage(Location liveWaypoint)
        {
            this.CloseWhenBackgroundIsClicked = true;

            this.InitializeComponent();

            this.BindingContext = new AddLiveWaypointPopupViewModel(liveWaypoint);
        }

        /// <summary>
        /// Shows "add live waypoint" popup page and lets the user edit the live waypoint
        /// properties.
        /// </summary>
        /// <param name="liveWaypoint">live waypoint to edit</param>
        /// <returns>entered text, or null when user canceled the popup dialog</returns>
        public static async Task<bool> ShowAsync(Location liveWaypoint)
        {
            var popupPage = new AddLiveWaypointPopupPage(liveWaypoint)
            {
                tcs = new TaskCompletionSource<bool>(),
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
                this.tcs.SetResult(false);
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
                this.tcs.SetResult(false);
            }

            return base.OnBackButtonPressed();
        }

        /// <summary>
        /// Called when user clicked on the "Add track" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnClickedAddLiveWaypointButton(object sender, EventArgs args)
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(true);
            }

            await this.Navigation.PopPopupAsync();
        }
    }
}
