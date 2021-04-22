using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Data service for Track objects
    /// </summary>
    public interface ITrackDataService
    {
        /// <summary>
        /// Adds a new track to the track list
        /// </summary>
        /// <param name="trackToAdd">track to add</param>
        /// <returns>task to wait on</returns>
        Task Add(Track trackToAdd);

        /// <summary>
        /// Retrieves a specific track
        /// </summary>
        /// <param name="trackId">track ID</param>
        /// <returns>track from list, or null when none was found</returns>
        Task<Track> Get(string trackId);

        /// <summary>
        /// Updates an existing track
        /// </summary>
        /// <param name="trackToUpdate">track to update</param>
        /// <returns>task to wait on</returns>
        Task Update(Track trackToUpdate);

        /// <summary>
        /// Removes a specific track
        /// </summary>
        /// <param name="trackId">track ID</param>
        /// <returns>task to wait on</returns>
        Task Remove(string trackId);

        /// <summary>
        /// Returns a list of all tracks
        /// </summary>
        /// <returns>list of tracks</returns>
        Task<IEnumerable<Track>> GetList();

        /// <summary>
        /// Adds new track list
        /// </summary>
        /// <param name="trackList">track list to add</param>
        /// <returns>task to wait on</returns>
        Task AddList(IEnumerable<Track> trackList);

        /// <summary>
        /// Clears list of tracks
        /// </summary>
        /// <returns>task to wait on</returns>
        Task ClearList();
    }
}
