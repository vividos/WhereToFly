using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page showing a waiting dialog. Show the dialog using ShowAsync(), hide again with
    /// HideAsync().
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WaitingPopupPage : PopupPage
    {
        /// <summary>
        /// Indicates if the popup page is currently shown
        /// </summary>
        private bool isShown;

        /// <summary>
        /// Creates a new waiting popup page
        /// </summary>
        /// <param name="waitingMessage">waiting message to display</param>
        public WaitingPopupPage(string waitingMessage)
        {
            this.CloseWhenBackgroundIsClicked = false;

            this.InitializeComponent();

            this.waitingMessage.Text = waitingMessage;
            this.isShown = false;
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
            return true;
        }
    }
}
