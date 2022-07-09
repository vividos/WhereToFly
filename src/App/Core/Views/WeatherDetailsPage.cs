using System.Threading.Tasks;
using WhereToFly.App.Core.Controls;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page showing weather details by showing an external web page. Also the page has quick
    /// select menus for all available weather pages shown on the dashboard.
    /// </summary>
    public class WeatherDetailsPage : ContentPage
    {
        /// <summary>
        /// Creates new weather details page
        /// </summary>
        /// <param name="iconDescription">weather icon description to open</param>
        public WeatherDetailsPage(WeatherIconDescription iconDescription)
        {
            this.OpenWebLink(iconDescription);

            this.AddRefreshToolbarButton();
            this.AddWeatherForecastToolbarButton();
            this.AddCurrentWeatherToolbarButton();
            this.AddWebcamsToolbarButton();
        }

        /// <summary>
        /// (Re-)opens web link from weather icon description
        /// </summary>
        /// <param name="iconDescription">weather icon description to open</param>
        private void OpenWebLink(WeatherIconDescription iconDescription)
        {
            this.Title = iconDescription.Name;

            var urlSource = new UrlWebViewSource
            {
                Url = iconDescription.WebLink,
            };

            if (this.Content == null)
            {
                var webView = new WeatherWebView
                {
                    Source = urlSource,

                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                };

                webView.AutomationId = "WeatherDetailsWebView";
                webView.Navigated += async (sender, args) => await this.OnNavigated_WebView(sender, args);

                this.Content = webView;
            }
            else
            {
                var webView = this.Content as WebView;
                webView.Source = urlSource;
            }
        }

        /// <summary>
        /// Called when navigation to current page has finished
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        /// <returns>task to wait on</returns>
        private async Task OnNavigated_WebView(object sender, WebNavigatedEventArgs args)
        {
            if (sender is WebView webView &&
                (args.Url.Contains("https://www.austrocontrol.at/flugwetter/start.php") ||
                args.Url.Contains("https://www.austrocontrol.at/flugwetter/timeout.php")))
            {
                await PasteAlpthermUsernamePassword(webView);
            }
        }

        /// <summary>
        /// Inserts alptherm username and password on the current web view.
        /// </summary>
        /// <param name="webView">web view to use</param>
        /// <returns>task to wait on</returns>
        private static async Task PasteAlpthermUsernamePassword(WebView webView)
        {
            string username = await SecureStorage.GetAsync(Constants.SecureSettingsAlpthermUsername);
            string password = await SecureStorage.GetAsync(Constants.SecureSettingsAlpthermPassword);

            if (string.IsNullOrEmpty(username) &&
                string.IsNullOrEmpty(password))
            {
                return;
            }

            string js = $"javascript:";

            if (!string.IsNullOrEmpty(username))
            {
                js += $"document.getElementById('username').value = '{username}';";
            }

            if (!string.IsNullOrEmpty(password))
            {
                js += $"document.getElementById('password').value = '{password}';";
            }

            if (!string.IsNullOrEmpty(username) &&
                !string.IsNullOrEmpty(password))
            {
                js += "document.getElementById('username').form.submit();";
            }

            await webView.EvaluateJavaScriptAsync(js);
        }

        /// <summary>
        /// Add "refresh" toolbar button to reload current web page
        /// </summary>
        private void AddRefreshToolbarButton()
        {
            ToolbarItem refreshButton = new ToolbarItem(
                "Refresh",
                Converter.ImagePathConverter.GetDeviceDependentImage("refresh"),
                () => this.OnClicked_ToolbarButtonRefresh(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "Refresh",
            };

            this.ToolbarItems.Add(refreshButton);
        }

        /// <summary>
        /// Called when user clicked on the refresh toolbar button
        /// </summary>
        private void OnClicked_ToolbarButtonRefresh()
        {
            var webView = this.Content as WebView;
            webView.Reload();
        }

        /// <summary>
        /// Add "weather forecast" toolbar button, in order to select new weather forecast web
        /// link.
        /// </summary>
        private void AddWeatherForecastToolbarButton()
        {
            ToolbarItem forecastButton = new ToolbarItem(
                "Forecast",
                Converter.ImagePathConverter.GetDeviceDependentImage("calendar_clock"),
                async () => await this.OnClicked_ToolbarButtonWeatherForecast(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "Forecast",
            };

            this.ToolbarItems.Add(forecastButton);
        }

        /// <summary>
        /// Called when user clicked on the "weather forecast" toolbar button
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonWeatherForecast()
        {
            await this.SelectAndOpenWeatherPageAsync("Weather forecast");
        }

        /// <summary>
        /// Add "current weather" toolbar button, in order to select new current weather web link.
        /// </summary>
        private void AddCurrentWeatherToolbarButton()
        {
            ToolbarItem weatherButton = new ToolbarItem(
                "Weather",
                Converter.ImagePathConverter.GetDeviceDependentImage("weather_partlycloudy"),
                async () => await this.OnClicked_ToolbarButtonCurrentWeather(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "Weather",
            };

            this.ToolbarItems.Add(weatherButton);
        }

        /// <summary>
        /// Called when user clicked on the "current weather" toolbar button
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonCurrentWeather()
        {
            await this.SelectAndOpenWeatherPageAsync("Current weather");
        }

        /// <summary>
        /// Add "webcam" toolbar button, in order to select new webcam web link.
        /// </summary>
        private void AddWebcamsToolbarButton()
        {
            ToolbarItem webcamsButton = new ToolbarItem(
                "Webcams",
                Converter.ImagePathConverter.GetDeviceDependentImage("camera"),
                async () => await this.OnClicked_ToolbarButtonWebcams(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "Webcams",
            };

            this.ToolbarItems.Add(webcamsButton);
        }

        /// <summary>
        /// Called when user clicked on the "webcams" toolbar button
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonWebcams()
        {
            await this.SelectAndOpenWeatherPageAsync("Webcams");
        }

        /// <summary>
        /// Lets the userselect a different weather icon description from given group and opens
        /// the new web link in the current web view.
        /// </summary>
        /// <param name="group">group to filter weather icon descriptions by</param>
        /// <returns>task to wait on</returns>
        private async Task SelectAndOpenWeatherPageAsync(string group)
        {
            var weatherIcon =
                await NavigationService.Instance.NavigateToPopupPageAsync<WeatherIconDescription>(
                    PopupPageKey.SelectWeatherIconPopupPage,
                    animated: true,
                    group);

            if (weatherIcon != null)
            {
                this.OpenWebLink(weatherIcon);
            }
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
