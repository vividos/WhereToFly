using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Data service for WeatherIconDescription objects
    /// </summary>
    public interface IWeatherIconDescriptionDataService
    {
        /// <summary>
        /// Adds a new weather icon description to the weather icon description list
        /// </summary>
        /// <param name="weatherIconDescriptionToAdd">weather icon description to add</param>
        /// <returns>task to wait on</returns>
        Task Add(WeatherIconDescription weatherIconDescriptionToAdd);

        /// <summary>
        /// Retrieves a specific weather icon description
        /// </summary>
        /// <param name="weatherIconDescriptionId">weather icon description ID</param>
        /// <returns>weather icon description from list, or null when none was found</returns>
        Task<WeatherIconDescription> Get(string weatherIconDescriptionId);

        /// <summary>
        /// Removes a specific weather icon description
        /// </summary>
        /// <param name="weatherIconDescriptionId">weather icon description ID</param>
        /// <returns>task to wait on</returns>
        Task Remove(string weatherIconDescriptionId);

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
}
