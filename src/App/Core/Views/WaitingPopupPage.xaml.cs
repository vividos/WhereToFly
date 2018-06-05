using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
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
        /// Creates a new waiting popup page
        /// </summary>
        /// <param name="waitingMessage">waiting message to display</param>
        public WaitingPopupPage(string waitingMessage)
        {
            this.CloseWhenBackgroundIsClicked = false;

            this.InitializeComponent();

            this.waitingMessage.Text = waitingMessage;
        }

        /// <summary>
        /// Shows popup page
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ShowAsync()
        {
            await this.Navigation.PushPopupAsync(this);
        }

        /// <summary>
        /// Hides popup page
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task HideAsync()
        {
            await this.Navigation.PopPopupAsync();
        }
    }
}
