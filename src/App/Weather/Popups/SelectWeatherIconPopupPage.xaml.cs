using CommunityToolkit.Maui.Views;
using WhereToFly.App.Weather.Models;
using WhereToFly.App.Weather.ViewModels;

namespace WhereToFly.App.Weather.Popups;

/// <summary>
/// Popup page for "select weather icon" function.
/// </summary>
public partial class SelectWeatherIconPopupPage : Popup<WeatherIconDescription>
{
    /// <summary>
    /// Creates a new web link selection popup page, without using grouping.
    /// </summary>
    public SelectWeatherIconPopupPage()
        : this(null)
    {
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
    /// Creates a new web link selection popup page
    /// </summary>
    /// <param name="group">
    /// weather icon group to filter by; may be null to show all groups
    /// </param>
    public SelectWeatherIconPopupPage(string? group)
    {
        this.BindingContext =
            new SelectWeatherIconViewModel(
                async (result) => await this.CloseAsync(result),
                group);

        this.InitializeComponent();

        // Workaround: CommunityToolkit.Maui 12.1.0 Popups don't pick up styles defined in
        // Styles.xaml, so set them here; can be removed as soon as this bug is fixed:
        // https://github.com/CommunityToolkit/Maui/issues/2747
        this.Margin = 0;
        this.SetAppThemeColor(
            BackgroundColorProperty,
            Color.FromArgb("#F5F5F5"),
            Color.FromArgb("#606164"));
    }
}
