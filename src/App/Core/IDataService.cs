using System;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
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
        /// Returns a data service for Location objects
        /// </summary>
        /// <returns>location data service</returns>
        ILocationDataService GetLocationDataService();

        /// <summary>
        /// Returns a data service for Track objects
        /// </summary>
        /// <returns>track data service</returns>
        ITrackDataService GetTrackDataService();

        /// <summary>
        /// Returns a data service for Layer objects
        /// </summary>
        /// <returns>layer data service</returns>
        ILayerDataService GetLayerDataService();

        /// <summary>
        /// Returns a data service for WeatherIconDescription objects. The data services manages
        /// all weather icon descriptions that are available.
        /// </summary>
        /// <returns>weather icon description data service</returns>
        IWeatherIconDescriptionDataService GetWeatherIconDescriptionDataService();

        /// <summary>
        /// Returns a data service for WeatherIconDescription objects that are visible on the
        /// weather dashboard.
        /// </summary>
        /// <returns>weather icon description data service</returns>
        IWeatherIconDescriptionDataService GetWeatherDashboardIconDataService();

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
        /// Retrieves latest info about a live track, including new list of track points and
        /// description.
        /// </summary>
        /// <param name="liveTrackId">live track ID</param>
        /// <param name="lastTrackPointTime">
        /// last track point that the client already has received, or null when no track points
        /// are known yet
        /// </param>
        /// <returns>query result for live track</returns>
        Task<LiveTrackQueryResult> GetLiveTrackDataAsync(
            string liveTrackId,
            DateTimeOffset? lastTrackPointTime);

        /// <summary>
        /// Plans a tour with given tour planning parameters and returns the planned tour.
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        /// <returns>planned tour</returns>
        Task<PlannedTour> PlanTourAsync(PlanTourParameters planTourParameters);
    }
}
