using System;
using System.Threading.Tasks;
using WhereToFly.App.Services;
using WhereToFly.App.Styles;
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
        /// The current app instance
        /// </summary>
        private static App App
            => App.Current as App
            ?? throw new InvalidOperationException("App.Current is not available");

        /// <summary>
        /// The current main page
        /// </summary>
        private static Page MainPage
            => App.MainPage
            ?? throw new InvalidOperationException("MainPage is not available");

        /// <summary>
        /// Gets or sets the user's selected app theme
        /// </summary>
        public AppTheme UserAppTheme
        {
            get => ThemeHelper.CurrentTheme;
            set => ThemeHelper.ChangeTheme(value, true);
        }

        /// <summary>
        /// Returns if the app is currently using a dark theme. This takes into account when the
        /// app theme is set to "Same as device".
        /// </summary>
        public bool IsDarkTheme
            => ThemeHelper.CurrentTheme == AppTheme.Dark;

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
                () => MainPage.DisplayToastAsync(
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
            => MainPage.DisplayAlert(
                Constants.AppTitle,
                message,
                cancel);

        /// <inheritdoc />
        public Task<bool> DisplayAlert(
            string message,
            string accept,
            string cancel)
            => MainPage.DisplayAlert(
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
            => MainPage.DisplayActionSheet(title, cancel, destruction, buttons);
    }
}
