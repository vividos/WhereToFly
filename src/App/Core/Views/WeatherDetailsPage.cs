using System.Threading.Tasks;
using WhereToFly.App.Model;
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
                Url = iconDescription.WebLink
            };

            if (this.Content == null)
            {
                var webView = new WebView
                {
                    Source = urlSource,

                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };

                webView.AutomationId = "WeatherDetailsWebView";

                this.Content = webView;
            }
            else
            {
                var webView = this.Content as WebView;
                webView.Source = urlSource;
            }
        }

        /// <summary>
        /// Add "refresh" toolbar button to reload current web page
        /// </summary>
        private void AddRefreshToolbarButton()
        {
            ToolbarItem refreshButton = new ToolbarItem(
                "Refresh",
                "refresh.xml",
                () => this.OnClicked_ToolbarButtonRefresh(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "Refresh"
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
                "calendar_clock.xml",
                async () => await this.OnClicked_ToolbarButtonWeatherForecast(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "Forecast"
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
                "weather_partlycloudy.xml",
                async () => await this.OnClicked_ToolbarButtonCurrentWeather(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "Weather"
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
                "camera.xml",
                async () => await this.OnClicked_ToolbarButtonWebcams(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "Webcams"
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
            var description = await SelectWeatherIconPopupPage.ShowAsync(group);

            if (description != null)
            {
                this.OpenWebLink(description);
            }
        }
    }
}
