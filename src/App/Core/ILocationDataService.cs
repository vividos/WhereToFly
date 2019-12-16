﻿using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.App.Model;

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
        /// Returns a list of all locations, possibly filtered
        /// </summary>
        /// <param name="filterText">filter text; may be null</param>
        /// <returns>list of locations</returns>
        Task<IEnumerable<Location>> GetList(string filterText = null);

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