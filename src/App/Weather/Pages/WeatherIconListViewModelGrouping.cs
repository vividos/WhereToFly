using WhereToFly.App.Weather.ViewModels;

namespace WhereToFly.App.Weather.Pages;

/// <summary>
/// Grouped list of weather icon list entry view models
/// </summary>
public class WeatherIconListViewModelGrouping : List<WeatherIconListEntryViewModel>
{
    /// <summary>
    /// Grouping key; the weather icon group
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Creates a new grouped list of weather icon list entry view models
    /// </summary>
    /// <param name="items">items to add to the list</param>
    public WeatherIconListViewModelGrouping(IEnumerable<WeatherIconListEntryViewModel> items)
    {
        this.Key = items.FirstOrDefault()?.Group ?? string.Empty;
        this.AddRange(items);
    }
}
