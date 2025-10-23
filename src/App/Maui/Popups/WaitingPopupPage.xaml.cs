using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using WhereToFly.App.Services;

namespace WhereToFly.App.Popups
{
    /// <summary>
    /// Popup page showing a waiting dialog. Show the dialog using ShowAsync(), hide again with
    /// HideAsync().
    /// </summary>
    public partial class WaitingPopupPage : Popup
    {
        /// <summary>
        /// Cancellation token source that cen be used to cancel a running task; may be null
        /// </summary>
        private readonly CancellationTokenSource? cancellationTokenSource;

        /// <summary>
        /// Indicates if the popup page is currently shown
        /// </summary>
        private bool isShown;

        /// <summary>
        /// Creates a new waiting popup page
        /// </summary>
        /// <param name="waitingMessage">waiting message to display</param>
        /// <param name="cancellationTokenSource">
        /// cancellation token source; when set, the waiting dialog has a cancel button that can
        /// cancel a running task
        /// </param>
        public WaitingPopupPage(
            string waitingMessage,
            CancellationTokenSource? cancellationTokenSource = null)
        {
            this.CanBeDismissedByTappingOutsideOfPopup =
                cancellationTokenSource != null;

            this.InitializeComponent();

            this.cancellationTokenSource = cancellationTokenSource;
            this.isShown = false;

            this.waitingMessage.Text = waitingMessage;
            this.cancelButton.IsVisible = cancellationTokenSource != null;

            // Workaround: CommunityToolkit.Maui 12.1.0 Popups don't pick up styles defined in
            // Styles.xaml, so set them here; can be removed as soon as this bug is fixed:
            // https://github.com/CommunityToolkit/Maui/issues/2747
            this.Margin = 0;
            this.SetAppThemeColor(
                BackgroundColorProperty,
                Color.FromArgb("#F5F5F5"),
                Color.FromArgb("#606164"));
        }

        /// <summary>
        /// Shows popup page
        /// </summary>
        public void Show()
        {
            UserInterface.MainPage.ShowPopup(
                this,
                NavigationService.DefaultPopupOptions);

            this.isShown = true;
        }

        /// <summary>
        /// Hides popup page
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task HideAsync()
        {
            try
            {
                if (this.isShown)
                {
                    await this.CloseAsync();
                }
            }
            catch (Exception)
            {
                // ignore when the waiting popup was already closed
            }

            this.isShown = false;
        }

        /// <summary>
        /// Called when the user clicked on the cancel button
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnClickedCancelButton(object sender, EventArgs args)
        {
            this.cancellationTokenSource?.Cancel();

            MainThread.BeginInvokeOnMainThread(
                async () => await this.CloseAsync());
        }
    }
}
