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
