using WhereToFly.App.Weather.Models;

namespace WhereToFly.App.Weather.Abstractions;

/// <summary>
/// Data service for weather dashboard icons
/// </summary>
public interface IWeatherDashboardIconDataService
{
    /// <summary>
    /// Adds a new weather icon description to the weather icon description list
    /// </summary>
    /// <param name="weatherIconDescriptionToAdd">weather icon description to add</param>
    /// <returns>task to wait on</returns>
    Task Add(WeatherIconDescription weatherIconDescriptionToAdd);

    /// <summary>
    /// Returns a list of all weather icon descriptions
    /// </summary>
    /// <returns>list of weather icon descriptions</returns>
    Task<IEnumerable<WeatherIconDescription>> GetList();

    /// <summary>
    /// Adds new weather icon description list
    /// </summary>
    /// <param name="weatherIconDescriptionList">weather icon description list to add</param>
    /// <returns>task to wait on</returns>
    Task AddList(IEnumerable<WeatherIconDescription> weatherIconDescriptionList);

    /// <summary>
    /// Clears list of weather icon descriptions
    /// </summary>
    /// <returns>task to wait on</returns>
    Task ClearList();
}
