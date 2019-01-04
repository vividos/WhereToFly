using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            var service = new FindMeSpotTrackerDataService();

            var liveWaypointData = await service.GetDataAsync(id);

            return new LiveWaypointQueryResult
            {
                Data = liveWaypointData,
                NextRequestDate = DateTimeOffset.Now + TimeSpan.FromMinutes(1.0) // TODO
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
