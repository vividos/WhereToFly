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
    }
}
