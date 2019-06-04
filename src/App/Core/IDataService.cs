using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Geo;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;

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

        /// <summary>
        /// Gets list of tracks
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>list of tracks</returns>
        Task<List<Track>> GetTrackListAsync(CancellationToken token);

        /// <summary>
        /// Stores new track list
        /// </summary>
        /// <param name="trackList">track list to store</param>
        /// <returns>task to wait on</returns>
        Task StoreTrackListAsync(List<Track> trackList);

        /// <summary>
        /// Gets list of layers
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>list of layers</returns>
        Task<List<Layer>> GetLayerListAsync(CancellationToken token);

        /// <summary>
        /// Stores new layer list
        /// </summary>
        /// <param name="layerList">layer list to store</param>
        /// <returns>task to wait on</returns>
        Task StoreLayerListAsync(List<Layer> layerList);

        /// <summary>
        /// Retrieves list of weather icon descriptions
        /// </summary>
        /// <returns>list with current weather icon descriptions</returns>
        Task<List<WeatherIconDescription>> GetWeatherIconDescriptionListAsync();

        /// <summary>
        /// Stores new weather icon list
        /// </summary>
        /// <param name="weatherIconList">weather icon list to store</param>
        /// <returns>task to wait on</returns>
        Task StoreWeatherIconDescriptionListAsync(List<WeatherIconDescription> weatherIconList);

        /// <summary>
        /// Returns the repository of all available weather icon descriptions that can be used
        /// to select weather icons for the customized list
        /// </summary>
        /// <returns>repository of all weather icons</returns>
        List<WeatherIconDescription> GetWeatherIconDescriptionRepository();

        /// <summary>
        /// Retrieves a favicon URL for the given website URL
        /// </summary>
        /// <param name="websiteUrl">website URL</param>
        /// <returns>favicon URL or empty string when none was found</returns>
        Task<string> GetFaviconUrlAsync(string websiteUrl);

        /// <summary>
        /// Retrieves latest info about a live waypoint, including new coordinates and
        /// description.
        /// </summary>
        /// <param name="liveWaypointId">live waypoint ID</param>
        /// <returns>query result for live waypoint</returns>
        Task<LiveWaypointQueryResult> GetLiveWaypointDataAsync(string liveWaypointId);

        /// <summary>
        /// Plans a tour with given tour planning parameters and returns the planned tour.
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        /// <returns>planned tour</returns>
        Task<PlannedTour> PlanTourAsync(PlanTourParameters planTourParameters);
    }
}
