using WhereToFly.App.Weather.Behaviors;
using WhereToFly.App.Weather.Models;
using WhereToFly.App.Weather.ViewModels;

namespace WhereToFly.App.Weather.Pages;

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
        }

        viewModel.OpenWebLink(iconDescription);
    }

    /// <summary>
    /// Called when the hardware back button is pressed; navigates back in browser, if
    /// possible, or lets page handle the navigation.
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
