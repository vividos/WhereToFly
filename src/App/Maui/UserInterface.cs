using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using WhereToFly.App.Abstractions;
using WhereToFly.App.Pages;
using WhereToFly.App.Services;

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
        internal static Page MainPage
            => App.Windows[0].Page
            ?? throw new InvalidOperationException("MainPage is not available");

        /// <summary>
        /// Gets or sets the user's selected app theme
        /// </summary>
        public AppTheme UserAppTheme
        {
            get => App.UserAppTheme;
            set
            {
                bool themeChanged = App.UserAppTheme != value;

                App.UserAppTheme = value;

                // refresh the menu page
                if (themeChanged &&
                    App.Windows.Count > 0 &&
                    MainPage is RootPage rootPage &&
                    rootPage.Flyout is MenuPage)
                {
                    rootPage.Flyout = new MenuPage();
                }
            }
        }

        /// <summary>
        /// Returns if the app is currently using a dark theme. This takes into account when the
        /// app theme is set to "Same as device".
        /// </summary>
        public bool IsDarkTheme
            => App.UserAppTheme == AppTheme.Dark ||
            (App.UserAppTheme == AppTheme.Unspecified && App.RequestedTheme == AppTheme.Dark);

        /// <summary>
        /// Returns current navigation service instance
        /// </summary>
        public INavigationService NavigationService =>
            Services.NavigationService.Instance;

        /// <summary>
        /// Displays toast message
        /// </summary>
        /// <param name="message">message text</param>
        public void DisplayToast(string message)
        {
#if WINDOWS
            MainThread.BeginInvokeOnMainThread(
                async () =>
                {
                    var toast = Toast.Make(
                        message,
                        ToastDuration.Short);

                    await toast.Show();
                });
#else
            MainThread.BeginInvokeOnMainThread(
                async () =>
                {
                    var options = new SnackbarOptions
                    {
                        BackgroundColor = Constants.PrimaryColor,
                        ActionButtonTextColor = Colors.White,
                        TextColor = Colors.White,
                    };

                    var snackbar = Snackbar.Make(
                        message,
                        duration: TimeSpan.FromSeconds(5),
                        visualOptions: options);

                    await snackbar.Show();
                });
#endif
        }

        /// <inheritdoc />
        public async Task DisplayAlert(string message, string cancel)
        {
            await MainPage.Dispatcher.DispatchAsync(async () =>
            {
                await MainPage.DisplayAlert(
                        Constants.AppTitle,
                        message,
                        cancel);
            });
        }

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
