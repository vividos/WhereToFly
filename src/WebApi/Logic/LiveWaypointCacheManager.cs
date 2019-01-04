using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.Shared.Model;
using WhereToFly.WebApi.Logic.Services;

namespace WhereToFly.WebApi.Logic
{
    /// <summary>
    /// Manager for live waypoint cache
    /// </summary>
    public class LiveWaypointCacheManager
    {
        /// <summary>
        /// Logger for cache manager
        /// </summary>
        private readonly ILogger<LiveWaypointCacheManager> logger;

        /// <summary>
        /// Cacle for live waypoint data, keyed by ID
        /// </summary>
        private readonly Dictionary<string, LiveWaypointData> liveWaypointCache = new Dictionary<string, LiveWaypointData>();

        /// <summary>
        /// Queue of live waypoints to process
        /// </summary>
        private readonly Queue<string> liveWaypointQueue = new Queue<string>();

        /// <summary>
        /// Lock object for cache and queue
        /// </summary>
        private readonly object lockCacheAndQueue = new object();

        /// <summary>
        /// Data service for querying Find Me SPOT service
        /// </summary>
        private readonly FindMeSpotTrackerDataService findMeSpotTrackerService = new FindMeSpotTrackerDataService();

        /// <summary>
        /// Data service for querying Garmin inReach services
        /// </summary>
        private readonly GarminInreachDataService garminInreachService = new GarminInreachDataService();

        /// <summary>
        /// Creates a new live waypoint cache manager object
        /// </summary>
        /// <param name="logger">logger instance to use</param>
        public LiveWaypointCacheManager(ILogger<LiveWaypointCacheManager> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Returns live waypoint data for given live waypoint ID. May throw an exception when the
        /// data is not readily available and must be fetched.
        /// </summary>
        /// <param name="id">live waypoint ID</param>
        /// <returns>live waypoint query result</returns>
        public async Task<LiveWaypointQueryResult> GetLiveWaypointData(string id)
        {
            var uri = new AppResourceUri(id);
            if (!uri.IsValid)
            {
                throw new ArgumentException("invalid live waypoint ID", nameof(id));
            }

            switch (uri.Type)
            {
                case AppResourceUri.ResourceType.FindMeSpotPos:
                    return await this.GetFindMeSpotPosResult(id);

                case AppResourceUri.ResourceType.GarminInreachPos:
                    return await this.GetGarminInreachPosResult(uri.Data);

                default:
                    Debug.Assert(false, "invalid app resource URI type");
                    return null;
            }
        }

        /// <summary>
        /// Gets query result for a Find Me SPOT live waypoint ID
        /// </summary>
        /// <param name="id">live waypoint ID</param>
        /// <returns>live waypoint query result</returns>
        private async Task<LiveWaypointQueryResult> GetFindMeSpotPosResult(string id)
        {
            var liveWaypointData = await this.findMeSpotTrackerService.GetDataAsync(id);

            return new LiveWaypointQueryResult
            {
                Data = liveWaypointData,
                NextRequestDate = this.findMeSpotTrackerService.GetNextRequestDate(id)
            };
        }

        /// <summary>
        /// Gets query result for a Garmin inReach live waypoint ID
        /// </summary>
        /// <param name="mapShareIdentifier">
        /// Garmin inReach MapShare identifier, part from AppResource URL
        /// </param>
        /// <returns>live waypoint query result</returns>
        private async Task<LiveWaypointQueryResult> GetGarminInreachPosResult(string mapShareIdentifier)
        {
            var liveWaypointData = await this.garminInreachService.GetDataAsync(mapShareIdentifier);

            return new LiveWaypointQueryResult
            {
                Data = liveWaypointData,
                NextRequestDate = this.garminInreachService.GetNextRequestDate(mapShareIdentifier)
            };
        }

        /// <summary>
        /// Caches (new or updated) live waypoint data
        /// </summary>
        /// <param name="data">live waypoint data to cache</param>
        public void CacheLiveWaypointData(LiveWaypointData data)
        {
            this.logger.LogDebug($"caching live waypoint data for id: {data.ID}");

            lock (this.lockCacheAndQueue)
            {
                this.liveWaypointCache[data.ID] = data;
            }
        }
    }
}
