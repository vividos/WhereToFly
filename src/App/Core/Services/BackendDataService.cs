using Refit;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// Data service that communicates with the backend data service (the project
    /// WhereToFly.WebApi.Core).
    /// </summary>
    public class BackendDataService
    {
        /// <summary>
        /// Interface to REST service for our backend web API
        /// </summary>
        internal interface IBackendWebApi
        {
            /// <summary>
            /// Returns the current app configuration to be used.
            /// </summary>
            /// <param name="appVersion">app version requesting the app configuration</param>
            /// <returns>app configuration</returns>
            [Get("/api/AppConfig?appVersion={appVersion}")]
            Task<AppConfig> GetAppConfigAsync(string appVersion);

            /// <summary>
            /// Returns a favicon URL representing the icon for a given website.
            /// </summary>
            /// <param name="websiteUrl">website to get favicon URL</param>
            /// <returns>favicon URL</returns>
            [Get("/api/FaviconUrl?websiteUrl={websiteUrl}")]
            Task<string> GetFaviconUrlAsync(string websiteUrl);

            /// <summary>
            /// Retrieves latest info about a live waypoint, including new coordinates and
            /// description.
            /// </summary>
            /// <param name="liveWaypointId">live waypoint ID</param>
            /// <returns>query result for live waypoint</returns>
            [Get("/api/LiveWaypoint?id={id}")]
            Task<LiveWaypointQueryResult> GetLiveWaypointDataAsync([AliasAs("id")] string liveWaypointId);

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
            [Get("/api/LiveTrack?id={id}&time={time}")]
            Task<LiveTrackQueryResult> GetLiveTrackDataAsync(
                [AliasAs("id")] string liveTrackId,
                [AliasAs("time")] DateTimeOffset? lastTrackPointTime);

            /// <summary>
            /// Plans a tour with given parameters and returns a planned tour, including
            /// description and a track.
            /// </summary>
            /// <param name="planTourParameters">tour planning parameters</param>
            /// <returns>planned tour</returns>
            [Post("/api/PlanTour")]
            Task<PlannedTour> PlanTourAsync([Body] PlanTourParameters planTourParameters);
        }

        /// <summary>
        /// Base URL for the WebApi REST web service
        /// </summary>
#pragma warning disable S1075 // URIs should not be hardcoded
        private const string BaseUrl = "https://wheretoflywebapi.azurewebsites.net";
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// Access to backend WebApi REST API
        /// </summary>
        private readonly IBackendWebApi backendWebApi;

        /// <summary>
        /// Creates a new backend data service object
        /// </summary>
        public BackendDataService()
        {
            this.backendWebApi = RestService.For<IBackendWebApi>(
                BaseUrl,
                new RefitSettings
                {
                    ContentSerializer = new NewtonsoftJsonContentSerializer(),
                });
        }

        /// <summary>
        /// Returns the current app configuration to be used.
        /// </summary>
        /// <param name="appVersion">app version requesting the app configuration</param>
        /// <returns>app configuration</returns>
        public async Task<AppConfig> GetAppConfigAsync(string appVersion)
        {
            return await this.backendWebApi.GetAppConfigAsync(appVersion);
        }

        /// <summary>
        /// Returns a favicon URL representing the icon for a given website.
        /// </summary>
        /// <param name="websiteUrl">website to get favicon URL</param>
        /// <returns>favicon URL</returns>
        public async Task<string> GetFaviconUrlAsync(string websiteUrl)
        {
            Debug.WriteLine($"Backend: Retrieving favicon for URL {websiteUrl}");
            return await this.backendWebApi.GetFaviconUrlAsync(websiteUrl);
        }

        /// <summary>
        /// Retrieves latest info about a live waypoint, including new coordinates and
        /// description.
        /// </summary>
        /// <param name="liveWaypointId">live waypoint ID</param>
        /// <returns>query result for live waypoint</returns>
        public async Task<LiveWaypointQueryResult> GetLiveWaypointDataAsync(string liveWaypointId)
        {
            LiveWaypointQueryResult result = await this.backendWebApi.GetLiveWaypointDataAsync(liveWaypointId);

            result.Data.ID = System.Net.WebUtility.UrlDecode(result.Data.ID);

            return result;
        }

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
        public async Task<LiveTrackQueryResult> GetLiveTrackDataAsync(
            string liveTrackId,
            DateTimeOffset? lastTrackPointTime)
        {
            LiveTrackQueryResult result = await this.backendWebApi.GetLiveTrackDataAsync(
                liveTrackId,
                lastTrackPointTime);

            result.Data.ID = System.Net.WebUtility.UrlDecode(result.Data.ID);

            return result;
        }

        /// <summary>
        /// Plans a tour with given parameters and returns a planned tour, including description
        /// and a track.
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        /// <returns>planned tour</returns>
        public async Task<PlannedTour> PlanTourAsync(PlanTourParameters planTourParameters)
        {
            return await this.backendWebApi.PlanTourAsync(planTourParameters);
        }
    }
}
