using Rg.Plugins.Popup.Extensions;
using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.MapView;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for setting parameters to show flying range.
    /// </summary>
    public partial class FlyingRangePopupPage : BasePopupPage
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly FlyingRangePopupViewModel viewModel;

        /// <summary>
        /// Task completion source to report back flying range parameters
        /// </summary>
        private TaskCompletionSource<FlyingRangeParameters> tcs;

        /// <summary>
        /// Creates a new popup page to edit flying range parameters
        /// </summary>
        public FlyingRangePopupPage()
        {
            this.CloseWhenBackgroundIsClicked = true;

            this.InitializeComponent();

            this.BindingContext = this.viewModel = new FlyingRangePopupViewModel(App.Settings);
        }

        /// <summary>
        /// Shows "flying parameters" popup page and lets the user edit the values.
        /// </summary>
        /// <returns>flying range parameters</returns>
        public static async Task<FlyingRangeParameters> ShowAsync()
        {
            var popupPage = new FlyingRangePopupPage()
            {
                tcs = new TaskCompletionSource<FlyingRangeParameters>(),
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

            Task.Run(this.viewModel.StoreFlyingRangeParameters);

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

            Task.Run(this.viewModel.StoreFlyingRangeParameters);

            return base.OnBackButtonPressed();
        }

        /// <summary>
        /// Called when user clicked on the "Show flying range" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnClickedShowFlyingRange(object sender, EventArgs args)
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(this.viewModel.Parameters);
            }

            await this.viewModel.StoreFlyingRangeParameters();

            await this.Navigation.PopPopupAsync();
        }
    }
}
