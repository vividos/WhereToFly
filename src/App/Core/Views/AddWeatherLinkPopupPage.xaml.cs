using Rg.Plugins.Popup.Extensions;
using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.ViewModels;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for adding a new weather link.
    /// </summary>
    public partial class AddWeatherLinkPopupPage : BasePopupPage
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly AddWeatherLinkPopupViewModel viewModel;

        /// <summary>
        /// Task completion source to report back weather icon description
        /// </summary>
        private TaskCompletionSource<WeatherIconDescription> tcs;

        /// <summary>
        /// Creates a new popup page to add weather link
        /// </summary>
        public AddWeatherLinkPopupPage()
        {
            this.CloseWhenBackgroundIsClicked = true;

            this.InitializeComponent();

            this.BindingContext = this.viewModel = new AddWeatherLinkPopupViewModel();
        }

        /// <summary>
        /// Shows "add weather link" popup page and lets the user type or paste a link.
        /// </summary>
        /// <returns>weather icon description, or null when the popup page was cancelled</returns>
        public static async Task<WeatherIconDescription> ShowAsync()
        {
            var popupPage = new AddWeatherLinkPopupPage
            {
                tcs = new TaskCompletionSource<WeatherIconDescription>(),
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
        /// Called when user clicked on the "Add weather link" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnClickedAddWeatherLinkButton(object sender, EventArgs args)
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(this.viewModel.WeatherIconDescription);
            }

            await this.Navigation.PopPopupAsync();
        }
    }
}
