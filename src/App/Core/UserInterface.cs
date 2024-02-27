using System.Threading.Tasks;
using WhereToFly.App.Services;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App
{
    /// <summary>
    /// User interface implementation for the Forms app
    /// </summary>
    internal class UserInterface : IUserInterface
    {
        /// <summary>
        /// Returns current navigation service instance
        /// </summary>
        public NavigationService NavigationService =>
            NavigationService.Instance;

        /// <summary>
        /// Displays toast message
        /// </summary>
        /// <param name="message">message text</param>
        public void DisplayToast(string message)
        {
            MainThread.BeginInvokeOnMainThread(
                () => App.Current.MainPage.DisplayToastAsync(
                    new ToastOptions
                    {
                        MessageOptions = new MessageOptions
                        {
                            Message = message,
                            Foreground = Color.White,
                        },
                        BackgroundColor = Constants.PrimaryColor,
                    }));
        }

        /// <inheritdoc />
        public Task DisplayAlert(string message, string cancel)
            => App.Current.MainPage.DisplayAlert(
                Constants.AppTitle,
                message,
                cancel);

        /// <inheritdoc />
        public Task<bool> DisplayAlert(
            string message,
            string accept,
            string cancel)
            => App.Current.MainPage.DisplayAlert(
                Constants.AppTitle,
                message,
                accept,
                cancel);

        /// <inheritdoc />
        public Task<string> DisplayActionSheet(
            string title,
            string? cancel,
            string? destruction,
            params string[] buttons)
            => App.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons);
    }
}
