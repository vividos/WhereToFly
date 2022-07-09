using Rg.Plugins.Popup.Extensions;
using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for adding a newly loaded track and edit its properties.
    /// </summary>
    public partial class AddTrackPopupPage : BasePopupPage
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly AddTrackPopupViewModel viewModel;

        /// <summary>
        /// Task completion source to report back edited track
        /// </summary>
        private TaskCompletionSource<Track> tcs;

        /// <summary>
        /// Creates a new popup page to edit track properties
        /// </summary>
        /// <param name="track">track to edit</param>
        public AddTrackPopupPage(Track track)
        {
            this.CloseWhenBackgroundIsClicked = true;

            this.InitializeComponent();

            this.BindingContext = this.viewModel = new AddTrackPopupViewModel(track);
        }

        /// <summary>
        /// Shows "add track" popup page and lets the user edit the track properties.
        /// </summary>
        /// <param name="track">track to edit</param>
        /// <returns>entered text, or null when user canceled the popup dialog</returns>
        public static async Task<Track> ShowAsync(Track track)
        {
            var popupPage = new AddTrackPopupPage(track)
            {
                tcs = new TaskCompletionSource<Track>(),
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
        /// Called when user clicked on the "Add track" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnClickedAddTrackButton(object sender, EventArgs args)
        {
            this.viewModel.UpdateTrack();

            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(this.viewModel.Track);
            }

            await this.Navigation.PopPopupAsync();
        }
    }
}
