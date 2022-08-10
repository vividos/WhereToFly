using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.Shared.Model;
using WhereToFly.WebApi.Logic.Services;

namespace WhereToFly.WebApi.Logic
{
    /// <summary>
    /// Manager for live track cache
    /// </summary>
    public class LiveTrackCacheManager
    {
        /// <summary>
        /// Time span of track points that are fetched for Garmin inReach live tracks
        /// </summary>
        private static readonly TimeSpan GarminInreachLiveTrackTimespan = TimeSpan.FromDays(30);

        /// <summary>
        /// Logger for cache manager
        /// </summary>
        private readonly ILogger<LiveTrackCacheManager> logger;

        /// <summary>
        /// Cache for live track data, keyed by ID
        /// </summary>
        private readonly Dictionary<string, LiveTrackData> liveTrackCache = new();

        /// <summary>
        /// Lock object for cache and queue
        /// </summary>
        private readonly object lockCacheAndQueue = new();

        /// <summary>
        /// Data service for querying Garmin inReach services
        /// </summary>
        private readonly GarminInreachDataService garminInreachService;

        /// <summary>
        /// Creates a new live track cache manager object
        /// </summary>
        /// <param name="logger">logger instance to use</param>
        /// <param name="garminInreachService">Garmin inReach service</param>
        public LiveTrackCacheManager(
            ILogger<LiveTrackCacheManager> logger,
            GarminInreachDataService garminInreachService)
        {
            this.logger = logger;
            this.garminInreachService = garminInreachService;
        }

        /// <summary>
        /// Returns live track data for given live track ID. May throw an exception when the
        /// data is not readily available and must be fetched.
        /// </summary>
        /// <param name="rawId">live track ID (maybe urlencoded)</param>
        /// <param name="lastTrackPointTime">
        /// last track point that the client already has received, or null when no track points
        /// are known yet
        /// </param>
        /// <returns>live track query result</returns>
        public async Task<LiveTrackQueryResult> GetLiveTrackDataAsync(
            string rawId,
            DateTimeOffset? lastTrackPointTime)
        {
            AppResourceUri uri = GetAndCheckLiveTrackId(rawId);

            LiveTrackQueryResult result = this.CheckCache(uri);
            if (result != null)
            {
                return result;
            }

            result = await this.GetLiveTrackQueryResult(uri);

            if (result?.Data != null)
            {
                this.CacheLiveTrackData(result.Data);
            }

            if (result != null &&
                lastTrackPointTime.HasValue &&
                lastTrackPointTime.Value > result.Data.TrackStart)
            {
                double lastTrackOffset = (lastTrackPointTime.Value - result.Data.TrackStart).TotalSeconds;
                result.Data.TrackPoints =
                    result.Data.TrackPoints
                    .Where(trackPoint => trackPoint.Offset > lastTrackOffset)
                    .ToArray();
            }

            return result;
        }

        /// <summary>
        /// Parses a raw live track ID and returns an AppResourceId object from it
        /// </summary>
        /// <param name="rawId">raw live track ID</param>
        /// <returns>live track ID as AppResourceUri object</returns>
        private static AppResourceUri GetAndCheckLiveTrackId(string rawId)
        {
            string id = System.Net.WebUtility.UrlDecode(rawId);
            var uri = new AppResourceUri(id);
            if (!uri.IsValid)
            {
                throw new ArgumentException("invalid live track ID", nameof(rawId));
            }

            return uri;
        }

        /// <summary>
        /// Checks next request date if a new request can be made; when not, returns cache entry
        /// when available.
        /// </summary>
        /// <param name="uri">live track ID</param>
        /// <returns>
        /// query result from cache, or null when there's no request in cache or when a request
        /// can be made online
        /// </returns>
        private LiveTrackQueryResult CheckCache(AppResourceUri uri)
        {
            if (this.IsNextRequestPossible(uri))
            {
                return null;
            }

            // ask cache
            string id = uri.ToString();

            LiveTrackData cachedData;
            lock (this.lockCacheAndQueue)
            {
                cachedData = this.liveTrackCache.ContainsKey(id) ? this.liveTrackCache[id] : null;
            }

            if (cachedData == null)
            {
                return null;
            }

            return new LiveTrackQueryResult
            {
                Data = cachedData,
                NextRequestDate = this.GetNextRequestDate(uri),
            };
        }

        /// <summary>
        /// Returns if next request is possible for the given app resource URI
        /// </summary>
        /// <param name="uri">live track ID</param>
        /// <returns>
        /// true when web service request is possible, false when cache should be used
        /// </returns>
        private bool IsNextRequestPossible(AppResourceUri uri)
        {
            switch (uri.Type)
            {
                case AppResourceUri.ResourceType.TestLiveTrack:
                    return true; // request is always possible

                case AppResourceUri.ResourceType.GarminInreachLiveTrack:
                    DateTimeOffset nextRequestDate = this.GetNextRequestDate(uri);
                    return nextRequestDate <= DateTimeOffset.Now;

                default:
                    Debug.Assert(false, "invalid app resource URI type");
                    return false;
            }
        }

        /// <summary>
        /// Returns next request date for live track in given ID
        /// </summary>
        /// <param name="uri">live track ID</param>
        /// <returns>date time offset of next possible request for this ID</returns>
        private DateTimeOffset GetNextRequestDate(AppResourceUri uri)
        {
            switch (uri.Type)
            {
                case AppResourceUri.ResourceType.TestLiveTrack:
                    return DateTimeOffset.Now.AddSeconds(30);

                case AppResourceUri.ResourceType.GarminInreachLiveTrack:
                    return this.garminInreachService.GetNextRequestDate(uri.Data);

                default:
                    Debug.Assert(false, "invalid app resource URI type");
                    return DateTimeOffset.MaxValue;
            }
        }

        /// <summary>
        /// Gets actual live track query result from web services
        /// </summary>
        /// <param name="uri">live track ID</param>
        /// <returns>query result</returns>
        private Task<LiveTrackQueryResult> GetLiveTrackQueryResult(AppResourceUri uri)
        {
            switch (uri.Type)
            {
                case AppResourceUri.ResourceType.TestLiveTrack:
                    return Task.FromResult(TestLiveTrackService.GetLiveTrackingQueryResult(uri.ToString()));

                case AppResourceUri.ResourceType.GarminInreachLiveTrack:
                    return this.GetGarminInreachLiveTrackResult(uri);

                default:
                    Debug.Assert(false, "invalid app resource URI type");
                    return Task.FromResult<LiveTrackQueryResult>(null);
            }
        }

        /// <summary>
        /// Returns a live track from a Garmin inReach device
        /// </summary>
        /// <param name="uri">live track ID to use</param>
        /// <returns>live track query result</returns>
        private async Task<LiveTrackQueryResult> GetGarminInreachLiveTrackResult(AppResourceUri uri)
        {
            string mapShareIdentifier = uri.Data;

            LiveTrackData liveTrackData =
                await this.garminInreachService.GetTrackAsync(
                    mapShareIdentifier,
                    DateTimeOffset.Now - GarminInreachLiveTrackTimespan);

            return new LiveTrackQueryResult
            {
                Data = liveTrackData,
                NextRequestDate = this.garminInreachService.GetNextRequestDate(mapShareIdentifier),
            };
        }
        /// <summary>
        /// Caches (new or updated) live track data
        /// </summary>
        /// <param name="data">live track data to cache</param>
        public void CacheLiveTrackData(LiveTrackData data)
        {
            this.logger.LogDebug($"caching live track data for id: {data.ID}");

            lock (this.lockCacheAndQueue)
            {
                this.liveTrackCache[data.ID] = data;
            }
        }
    }
}
