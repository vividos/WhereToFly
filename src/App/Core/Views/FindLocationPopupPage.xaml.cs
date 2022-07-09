using Rg.Plugins.Popup.Extensions;
using System;
using System.Threading.Tasks;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for "Find location" function in order to input a location name.
    /// </summary>
    public partial class FindLocationPopupPage : BasePopupPage
    {
        /// <summary>
        /// Task completion source to report back typed in location name to a task.
        /// </summary>
        private TaskCompletionSource<string> tcs;

        /// <summary>
        /// Creates a new popup page
        /// </summary>
        public FindLocationPopupPage()
        {
            this.CloseWhenBackgroundIsClicked = true;

            this.InitializeComponent();
        }

        /// <summary>
        /// Shows "Find location" popup page, lets the user enter text and returns the text.
        /// </summary>
        /// <returns>entered text, or null when user canceled the popup dialog</returns>
        public static async Task<string> ShowAsync()
        {
            var popupPage = new FindLocationPopupPage
            {
                tcs = new TaskCompletionSource<string>(),
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
        /// Called when user clicked on the "Find" button, starting the search
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnClickedFindButton(object sender, EventArgs args)
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(this.locationEntry.Text);
            }

            await this.Navigation.PopPopupAsync();
        }

        /// <summary>
        /// Called when popup page is about to appear; sets focus to the entry field.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            this.locationEntry.Focus();
        }
    }
}
