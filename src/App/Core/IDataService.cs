using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Model;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Data service for the app; provides access to data storage.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Gets the current app settings object
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>app settings object</returns>
        Task<AppSettings> GetAppSettingsAsync(CancellationToken token);

        /// <summary>
        /// Stores new app settings object
        /// </summary>
        /// <param name="appSettings">new app settings to store</param>
        /// <returns>task to wait on</returns>
        Task StoreAppSettingsAsync(AppSettings appSettings);

        /// <summary>
        /// Gets list of locations
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>list of locations</returns>
        Task<List<Location>> GetLocationListAsync(CancellationToken token);

        /// <summary>
        /// Stores new location list
        /// </summary>
        /// <param name="locationList">location list to store</param>
        /// <returns>task to wait on</returns>
        Task StoreLocationListAsync(List<Location> locationList);
    }
}
