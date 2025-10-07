using WhereToFly.App.Behaviors;
using WhereToFly.App.Models;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.Pages
{
    /// <summary>
    /// Page showing weather details by showing an external web page. Also the page has quick
    /// select menus for all available weather pages shown on the dashboard.
    /// </summary>
    public partial class WeatherDetailsPage : ContentPage
    {
        /// <summary>
        /// Creates new weather details page
        /// </summary>
        /// <param name="iconDescription">weather icon description to open</param>
        public WeatherDetailsPage(WeatherIconDescription iconDescription)
        {
            this.InitializeComponent();

            var viewModel = new WeatherDetailsViewModel(this.weatherWebView);

            this.BindingContext = viewModel;

            if (this.weatherWebView.Behaviors.Count == 0)
            {
#if ANROID || WINDOWS
                this.weatherWebView.Behaviors.Add(
                    new WebViewLongTapToSaveImageBehavior());
#endif

                this.weatherWebView.Navigated +=
                    async (sender, args) =>
                    await OnNavigated_WebView(sender, args);
            }

            viewModel.OpenWebLink(iconDescription);
        }

        /// <summary>
        /// Called when navigation to current page has finished
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        /// <returns>task to wait on</returns>
        private static async Task OnNavigated_WebView(
            object? sender,
            WebNavigatedEventArgs args)
        {
            if (sender is WebView webView &&
                args.Url.StartsWith("https://www.austrocontrol.at/flugwetter/"))
            {
                await CheckAlpthermLogin(webView);
            }
        }

        /// <summary>
        /// Checks if login for alptherm is needed on the current web view.
        /// </summary>
        /// <param name="webView">web view to use</param>
        /// <returns>task to wait on</returns>
        private static async Task CheckAlpthermLogin(IWebView webView)
        {
            try
            {
                string result = await webView.EvaluateJavaScriptAsync(
                    "javascript:document.getElementById('login-form') !== null;");

                if (result == "true")
                {
                    await PasteAlpthermUsernamePassword(webView);
                }
            }
            catch (Exception)
            {
                // ignore errors from check
            }
        }

        /// <summary>
        /// Inserts alptherm username and password on the current web view.
        /// </summary>
        /// <param name="webView">web view to use</param>
        /// <returns>task to wait on</returns>
        private static async Task PasteAlpthermUsernamePassword(IWebView webView)
        {
            string? username = await SecureStorage.GetAsync(Constants.SecureSettingsAlpthermUsername);
            string? password = await SecureStorage.GetAsync(Constants.SecureSettingsAlpthermPassword);

            if (string.IsNullOrEmpty(username) &&
                string.IsNullOrEmpty(password))
            {
                return;
            }

            string js = $"javascript:";

            if (!string.IsNullOrEmpty(username))
            {
                js += $"document.getElementById('httpd_username').value = '{username}';";
            }

            if (!string.IsNullOrEmpty(password))
            {
                js += $"document.getElementById('httpd_password').value = '{password}';";
            }

            if (!string.IsNullOrEmpty(username) &&
                !string.IsNullOrEmpty(password))
            {
                js += "document.getElementById('httpd_username').form.submit();";
            }

            await webView.EvaluateJavaScriptAsync(js);
        }

        /// <summary>
        /// Called when the hardware back button is pressed; navigates back in browser, if
        /// possible, or lets Forms handle the navigation.
        /// </summary>
        /// <returns>true when the back button press was handled, false when not</returns>
        protected override bool OnBackButtonPressed()
        {
            if (this.Content is WebView webView &&
                webView.CanGoBack)
            {
                webView.GoBack();
                return true;
            }

            return base.OnBackButtonPressed();
        }
    }
}
