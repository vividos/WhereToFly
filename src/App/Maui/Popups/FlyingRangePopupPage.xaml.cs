using WhereToFly.App.MapView.Models;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.Popups
{
    /// <summary>
    /// Popup page for setting parameters to show flying range.
    /// </summary>
    public partial class FlyingRangePopupPage : BasePopupPage<FlyingRangeParameters>
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly FlyingRangePopupViewModel viewModel;

        /// <summary>
        /// Creates a new popup page to edit flying range parameters
        /// </summary>
        public FlyingRangePopupPage()
        {
            this.InitializeComponent();

            this.BindingContext = this.viewModel = new FlyingRangePopupViewModel(App.Settings!);

            this.Closed += this.OnClosed;
        }

        /// <summary>
        /// Called when user clicked on the "Show flying range" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnClickedShowFlyingRange(object sender, EventArgs args)
        {
            this.SetResult(this.viewModel.Parameters);
        }

        /// <summary>
        /// Called when the popup was closed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnClosed(object? sender, EventArgs args)
        {
            this.Closed -= this.OnClosed;

            await this.viewModel.StoreFlyingRangeParameters();
        }
    }
}
