using WhereToFly.App.MapView;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.Views
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
        /// <param name="result">result object</param>
        /// <param name="wasDismissedByTappingOutsideOfPopup">
        /// true when dismissed by tapping outside of popup
        /// </param>
        /// <param name="token">cancellation token; unused</param>
        /// <returns>task to wait on</returns>
        protected override async Task OnClosed(
            object? result,
            bool wasDismissedByTappingOutsideOfPopup,
            CancellationToken token = default)
        {
            await this.viewModel.StoreFlyingRangeParameters();

            await base.OnClosed(result, wasDismissedByTappingOutsideOfPopup, token);
        }
    }
}
