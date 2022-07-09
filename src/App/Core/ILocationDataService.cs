using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Data service for Location objects
    /// </summary>
    public interface ILocationDataService
    {
        /// <summary>
        /// Adds a new location to the location list
        /// </summary>
        /// <param name="locationToAdd">location to add</param>
        /// <returns>task to wait on</returns>
        Task Add(Location locationToAdd);

        /// <summary>
        /// Retrieves a specific location
        /// </summary>
        /// <param name="locationId">location ID</param>
        /// <returns>location from list, or null when none was found</returns>
        Task<Location> Get(string locationId);

        /// <summary>
        /// Updates an existing location in the location list
        /// </summary>
        /// <param name="location">location to update</param>
        /// <returns>task to wait on</returns>
        Task Update(Location location);

        /// <summary>
        /// Removes a specific location
        /// </summary>
        /// <param name="locationId">location ID</param>
        /// <returns>task to wait on</returns>
        Task Remove(string locationId);

        /// <summary>
        /// Returns if the location list is empty
        /// </summary>
        /// <returns>true when list is empty, false when not</returns>
        Task<bool> IsListEmpty();

        /// <summary>
        /// Returns a list of all locations, possibly filtered
        /// </summary>
        /// <param name="filterSettings">filter settings; may be null</param>
        /// <returns>list of locations</returns>
        Task<IEnumerable<Location>> GetList(LocationFilterSettings filterSettings = null);

        /// <summary>
        /// Adds new location list
        /// </summary>
        /// <param name="locationList">location list to add</param>
        /// <returns>task to wait on</returns>
        Task AddList(IEnumerable<Location> locationList);

        /// <summary>
        /// Clears list of locations
        /// </summary>
        /// <returns>task to wait on</returns>
        Task ClearList();
    }
}
