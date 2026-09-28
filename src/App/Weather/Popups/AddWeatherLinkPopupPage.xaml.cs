using CommunityToolkit.Maui.Views;
using WhereToFly.App.Weather.Models;
using WhereToFly.App.Weather.ViewModels;

namespace WhereToFly.App.Weather.Popups;

/// <summary>
/// Popup page for adding a new weather link.
/// </summary>
public partial class AddWeatherLinkPopupPage : Popup<WeatherIconDescription?>
{
    /// <summary>
    /// View model for this popup page
    /// </summary>
    private readonly AddWeatherLinkPopupViewModel viewModel;

    /// <summary>
    /// Creates a new popup page to add weather link
    /// </summary>
    public AddWeatherLinkPopupPage()
    {
        this.InitializeComponent();

        this.BindingContext = this.viewModel = new AddWeatherLinkPopupViewModel();

        // Workaround: CommunityToolkit.Maui 12.1.0 Popups don't pick up styles defined in
        // Styles.xaml, so set them here; can be removed as soon as this bug is fixed:
        // https://github.com/CommunityToolkit/Maui/issues/2747
        this.Margin = 0;
        this.SetAppThemeColor(
            BackgroundColorProperty,
            Color.FromArgb("#F5F5F5"),
            Color.FromArgb("#606164"));
    }

    /// <summary>
    /// Called when user clicked on the "Add weather link" button, ending the popup page.
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="args">event args</param>
    private void OnClickedAddWeatherLinkButton(object? sender, EventArgs args)
    {
        MainThread.BeginInvokeOnMainThread(
            async () => await this.CloseAsync(this.viewModel.WeatherIconDescription));
    }
}
