using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
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
        /// Cache for live waypoint data, keyed by ID
        /// </summary>
        private readonly Dictionary<string, LiveWaypointData> liveWaypointCache = new();

        /// <summary>
        /// Lock object for cache and queue
        /// </summary>
        private readonly object lockCacheAndQueue = new();

        /// <summary>
        /// Data service for querying Find Me SPOT service
        /// </summary>
        private readonly FindMeSpotTrackerDataService findMeSpotTrackerService = new();

        /// <summary>
        /// Data service for querying Garmin inReach services
        /// </summary>
        private readonly GarminInreachDataService garminInreachService;

        /// <summary>
        /// Creates a new live waypoint cache manager object
        /// </summary>
        /// <param name="logger">logger instance to use</param>
        /// <param name="garminInreachService">Garmin inReach service</param>
        public LiveWaypointCacheManager(
            ILogger<LiveWaypointCacheManager> logger,
            GarminInreachDataService garminInreachService)
        {
            this.logger = logger;
            this.garminInreachService = garminInreachService;
        }

        /// <summary>
        /// Returns live waypoint data for given live waypoint ID. May throw an exception when the
        /// data is not readily available and must be fetched.
        /// </summary>
        /// <param name="rawId">live waypoint ID (maybe urlencoded)</param>
        /// <returns>live waypoint query result</returns>
        public async Task<LiveWaypointQueryResult> GetLiveWaypointData(string rawId)
        {
            AppResourceUri uri = GetAndCheckLiveWaypointId(rawId);

            LiveWaypointQueryResult result = this.CheckCache(uri);
            if (result != null)
            {
                return result;
            }

            result = await this.GetLiveWaypointQueryResult(uri);

            if (result != null)
            {
                this.CacheLiveWaypointData(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Parses a raw live waypoint ID and returns an AppResourceId object from it
        /// </summary>
        /// <param name="rawId">raw live waypoint ID</param>
        /// <returns>live waypoint ID as AppResourceUri object</returns>
        private static AppResourceUri GetAndCheckLiveWaypointId(string rawId)
        {
            string id = System.Net.WebUtility.UrlDecode(rawId);
            var uri = new AppResourceUri(id);
            if (!uri.IsValid)
            {
                throw new ArgumentException("invalid live waypoint ID", nameof(rawId));
            }

            return uri;
        }

        /// <summary>
        /// Checks next request date if a new request can be made; when not, returns cache entry
        /// when available.
        /// </summary>
        /// <param name="uri">live waypoint ID</param>
        /// <returns>
        /// query result from cache, or null when there's no request in cache or when a request
        /// can be made online
        /// </returns>
        private LiveWaypointQueryResult CheckCache(AppResourceUri uri)
        {
            if (this.IsNextRequestPossible(uri))
            {
                return null;
            }

            // ask cache
            string id = uri.ToString();

            LiveWaypointData cachedData;
            lock (this.lockCacheAndQueue)
            {
                cachedData = this.liveWaypointCache.ContainsKey(id) ? this.liveWaypointCache[id] : null;
            }

            if (cachedData == null)
            {
                return null;
            }

            return new LiveWaypointQueryResult
            {
                Data = cachedData,
                NextRequestDate = this.GetNextRequestDate(uri),
            };
        }

        /// <summary>
        /// Returns if next request is possible for the given app resource URI
        /// </summary>
        /// <param name="uri">live waypoint ID</param>
        /// <returns>
        /// true when web service request is possible, false when cache should be used
        /// </returns>
        private bool IsNextRequestPossible(AppResourceUri uri)
        {
            switch (uri.Type)
            {
                case AppResourceUri.ResourceType.FindMeSpotPos:
                case AppResourceUri.ResourceType.GarminInreachPos:
                    DateTimeOffset nextRequestDate = this.GetNextRequestDate(uri);
                    return nextRequestDate <= DateTimeOffset.Now;

                case AppResourceUri.ResourceType.TestPos:
                    return true; // request is always possible

                default:
                    Debug.Assert(false, "invalid app resource URI type");
                    return false;
            }
        }

        /// <summary>
        /// Returns next request date for live waypoint in given ID
        /// </summary>
        /// <param name="uri">live waypoint ID</param>
        /// <returns>date time offset of next possible request for this ID</returns>
        private DateTimeOffset GetNextRequestDate(AppResourceUri uri)
        {
            switch (uri.Type)
            {
                case AppResourceUri.ResourceType.FindMeSpotPos:
                    return this.findMeSpotTrackerService.GetNextRequestDate(uri);

                case AppResourceUri.ResourceType.GarminInreachPos:
                    return this.garminInreachService.GetNextRequestDate(uri.Data);

                case AppResourceUri.ResourceType.TestPos:
                    return DateTimeOffset.Now + TimeSpan.FromMinutes(1.0);

                default:
                    Debug.Assert(false, "invalid app resource URI type");
                    return DateTimeOffset.MaxValue;
            }
        }

        /// <summary>
        /// Gets actual live waypoint query result from web services
        /// </summary>
        /// <param name="uri">live waypoint ID</param>
        /// <returns>query result</returns>
        private async Task<LiveWaypointQueryResult> GetLiveWaypointQueryResult(AppResourceUri uri)
        {
            switch (uri.Type)
            {
                case AppResourceUri.ResourceType.FindMeSpotPos:
                    return await this.GetFindMeSpotPosResult(uri);

                case AppResourceUri.ResourceType.GarminInreachPos:
                    return await this.GetGarminInreachPosResult(uri);

                case AppResourceUri.ResourceType.TestPos:
                    return await this.GetTestPosResult(uri);

                default:
                    Debug.Assert(false, "invalid app resource URI type");
                    return null;
            }
        }

        /// <summary>
        /// Gets query result for a Find Me SPOT live waypoint ID
        /// </summary>
        /// <param name="uri">live waypoint ID</param>
        /// <returns>live waypoint query result</returns>
        private async Task<LiveWaypointQueryResult> GetFindMeSpotPosResult(AppResourceUri uri)
        {
            var liveWaypointData = await this.findMeSpotTrackerService.GetDataAsync(uri);

            return new LiveWaypointQueryResult
            {
                Data = liveWaypointData,
                NextRequestDate = this.findMeSpotTrackerService.GetNextRequestDate(uri),
            };
        }

        /// <summary>
        /// Gets query result for a Garmin inReach live waypoint ID
        /// </summary>
        /// <param name="uri">live waypoint Id</param>
        /// <returns>live waypoint query result</returns>
        private async Task<LiveWaypointQueryResult> GetGarminInreachPosResult(AppResourceUri uri)
        {
            string mapShareIdentifier = uri.Data;

            var liveWaypointData = await this.garminInreachService.GetDataAsync(mapShareIdentifier);

            return new LiveWaypointQueryResult
            {
                Data = liveWaypointData,
                NextRequestDate = this.garminInreachService.GetNextRequestDate(mapShareIdentifier),
            };
        }

        /// <summary>
        /// Returns a test position, based on the current time
        /// </summary>
        /// <param name="uri">live waypoint ID to use</param>
        /// <returns>live waypoint query result</returns>
        private Task<LiveWaypointQueryResult> GetTestPosResult(AppResourceUri uri)
        {
            MapPoint mapPoint;
            switch (uri.Data.ToLowerInvariant())
            {
                case "crossingthealps2019":
                    var point1 = new MapPoint(47.754076, 12.352277, 0.0); // Kampenwand
                    var point2 = new MapPoint(46.017779, 11.900711, 0.0); // Feltre

                    // every 10 minutes, interpolate between two points, in interval [-0.5; 1.5[
                    double t = (((DateTimeOffset.Now.TimeOfDay.TotalMinutes % 10.0) / 10.0) * 2) - 0.5;

                    mapPoint = new MapPoint(
                        Shared.Base.Math.Interpolate(point1.Latitude, point2.Latitude, t),
                        Shared.Base.Math.Interpolate(point1.Longitude, point2.Longitude, t),
                        0.0);

                    break;

                case "data":
                case "spitzingsee":
                    mapPoint = new MapPoint(47.664601, 11.885455, 0.0);

                    double timeAngleInDegrees = (DateTimeOffset.Now.TimeOfDay.TotalMinutes * 6.0) % 360;
                    double timeAngle = timeAngleInDegrees / 180.0 * Math.PI;
                    mapPoint.Latitude += 0.025 * Math.Sin(timeAngle);
                    mapPoint.Longitude -= 0.025 * Math.Cos(timeAngle);
                    break;

                case "livetracking":
                    return Task.FromResult(
                        TestLiveTrackService.GetPositionQueryResult(uri.ToString()));

                default:
                    throw new ArgumentException("invalid TestPos uri data");
            }

            DateTimeOffset nextRequestDate = DateTimeOffset.Now + TimeSpan.FromMinutes(1.0);

            return Task.FromResult(
                new LiveWaypointQueryResult
                {
                    Data = new LiveWaypointData
                    {
                        ID = uri.ToString(),
                        TimeStamp = DateTimeOffset.Now,
                        Longitude = mapPoint.Longitude,
                        Latitude = mapPoint.Latitude,
                        Altitude = mapPoint.Altitude.Value,
                        Name = "Live waypoint test position",
                        Description = "Hello from the Where-to-fly backend services!<br/>" +
                            $"Next request date is {nextRequestDate}" +
                            GetCoveredDistanceDescription(mapPoint),
                        DetailsLink = string.Empty,
                    },
                    NextRequestDate = nextRequestDate,
                });
        }

        /// <summary>
        /// Calculates "covered distance" description text based on the current point
        /// </summary>
        /// <param name="mapPoint">current point</param>
        /// <returns>description text to display</returns>
        public static string GetCoveredDistanceDescription(MapPoint mapPoint)
        {
            var point1 = new MapPoint(47.754076, 12.352277, 0.0); // Kampenwand
            var point2 = new MapPoint(46.017779, 11.900711, 0.0); // Feltre

            double distanceTo1 = mapPoint.DistanceTo(point1);
            double distanceTo2 = mapPoint.DistanceTo(point2);
            double distanceBetween12 = point1.DistanceTo(point2);

            // between the two points? (or in an circle spanning from point1 to point2
            if (distanceTo1 < distanceBetween12 &&
                distanceTo2 < distanceBetween12)
            {
                double percent = (distanceTo1 / distanceBetween12) * 100.0;
                return $"<br/>Covered distance: {distanceTo1 / 1000.0:0.0} km, {percent:0.0}% done!";
            }
            else if (distanceTo1 < distanceTo2)
            {
                return $"<br/>Before start turnpoint ({distanceTo1 / 1000.0:0.0} km to start)!";
            }
            else
            {
                return $"<br/>End turnpoint reached ({distanceTo2 / 1000.0:0.0} km from end)!";
            }
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
