using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page showing a waiting dialog. Show the dialog using ShowAsync(), hide again with
    /// HideAsync().
    /// </summary>
    public partial class WaitingPopupPage : PopupPage
    {
        /// <summary>
        /// Cancellation token source that cen be used to cancel a running task; may be null
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource;

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
        public WaitingPopupPage(string waitingMessage, CancellationTokenSource cancellationTokenSource = null)
        {
            this.CloseWhenBackgroundIsClicked = cancellationTokenSource != null;

            this.InitializeComponent();

            this.cancellationTokenSource = cancellationTokenSource;
            this.isShown = false;

            this.waitingMessage.Text = waitingMessage;
            this.cancelButton.IsVisible = cancellationTokenSource != null;
        }

        /// <summary>
        /// Shows popup page
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ShowAsync()
        {
            await this.Navigation.PushPopupAsync(this);

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
                    await this.Navigation.PopPopupAsync();
                }
            }
            catch (IndexOutOfRangeException)
            {
                // ignore when the waiting popup was already closed
            }

            this.isShown = false;
        }

        /// <summary>
        /// Called when user naviaged back with the back button, dismissing the popup page.
        /// </summary>
        /// <returns>true in order to disable action on back button press</returns>
        protected override bool OnBackButtonPressed()
        {
            if (this.cancellationTokenSource == null)
            {
                return true;
            }

            this.cancellationTokenSource.Cancel();

            this.isShown = false;

            return false;
        }

        /// <summary>
        /// Called when the user clicked on the cancel button
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnClickedCancelButton(object sender, EventArgs args)
        {
            Debug.Assert(
                this.cancellationTokenSource != null,
                "button can only be clicked when cancellation token source is available");

            this.cancellationTokenSource?.Cancel();
        }
    }
}
