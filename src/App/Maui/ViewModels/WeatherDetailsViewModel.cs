using System.Windows.Input;
using WhereToFly.App.Models;

namespace WhereToFly.App.ViewModels;

/// <summary>
/// View model for the weather details page
/// </summary>
internal class WeatherDetailsViewModel : ViewModelBase
{
    /// <summary>
    /// Weather web view
    /// </summary>
    private readonly IWebView weatherWebView;

    #region Binding properties

    /// <summary>
    /// Title text
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Web view source for the weather web view
    /// </summary>
    public WebViewSource? WeatherWebViewSource { get; private set; }

    /// <summary>
    /// Refresh toolbar command
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Weather forecast toolbar command
    /// </summary>
    public ICommand WeatherForecastCommand { get; }

    /// <summary>
    /// Current weather toolbar command
    /// </summary>
    public ICommand CurrentWeatherCommand { get; }

    /// <summary>
    /// Webcams toolbar command
    /// </summary>
    public ICommand WebcamsCommand { get; }
    #endregion

    /// <summary>
    /// Creates a new view model
    /// </summary>
    /// <param name="weatherWebView">weather web view implementation</param>
    public WeatherDetailsViewModel(IWebView weatherWebView)
    {
        this.weatherWebView = weatherWebView;

        this.RefreshCommand = new Command(
            () => this.weatherWebView.Reload());

        this.WeatherForecastCommand = new AsyncRelayCommand(
            async () => await this.SelectAndOpenWeatherPageAsync("Weather forecast"));

        this.CurrentWeatherCommand = new AsyncRelayCommand(
            async () => await this.SelectAndOpenWeatherPageAsync("Current weather"));

        this.WebcamsCommand = new AsyncRelayCommand(
            async () => await this.SelectAndOpenWeatherPageAsync("Webcams"));
    }

    /// <summary>
    /// (Re-)opens web link from weather icon description
    /// </summary>
    /// <param name="iconDescription">weather icon description to open</param>
    public void OpenWebLink(WeatherIconDescription iconDescription)
    {
        this.Title = iconDescription.Name;
        this.OnPropertyChanged(nameof(this.Title));

        this.WeatherWebViewSource = new UrlWebViewSource
        {
            Url = iconDescription.WebLink,
        };

        this.OnPropertyChanged(nameof(this.WeatherWebViewSource));
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
            await UserInterface.NavigationService.NavigateToPopupPageAsync<WeatherIconDescription>(
                PopupPageKey.SelectWeatherIconPopupPage,
                animated: true,
                group);

        if (weatherIcon != null)
        {
            this.OpenWebLink(weatherIcon);
        }
    }
}
