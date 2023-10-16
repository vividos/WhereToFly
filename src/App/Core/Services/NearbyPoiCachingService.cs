using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.MapView;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// Caching nearby POI service that uses the backend data service to load more POIs
    /// </summary>
    public class NearbyPoiCachingService : INearbyPoiService
    {
        /// <summary>
        /// Filename of cache file to read from
        /// </summary>
        private const string CacheFilename = "nearbyPoiCache.json";

        /// <summary>
        /// Backend data service to retrieve more nearby POIs when not in cache
        /// </summary>
        private readonly BackendDataService backendDataService;

        /// <summary>
        /// Folder where cache file is stored
        /// </summary>
        private readonly string cacheFolder;

        /// <summary>
        /// The cache dictionary
        /// </summary>
        private Dictionary<LatLongKey, List<Location>> cache = new();

        /// <summary>
        /// Creates a new nearby POI caching service
        /// </summary>
        /// <param name="backendDataService">
        /// backend data service to use for getting more nearby POIs; must not be null
        /// </param>
        /// <param name="cacheFolder">cache folder to load and store POIs; must exist</param>
        public NearbyPoiCachingService(
            BackendDataService backendDataService,
            string cacheFolder)
        {
            Debug.Assert(
                Directory.Exists(cacheFolder),
                "cache folder must already exist");

            this.backendDataService = backendDataService;
            this.cacheFolder = cacheFolder;

            this.LoadCache();
        }

        /// <summary>
        /// Gets nearby POIs in the given rectangle area; may load locations from cache or from
        /// the backend service. May throw an exception when no network connectivity is present.
        /// </summary>
        /// <param name="area">map rectangle area</param>
        /// <param name="visiblePoiIds">list of IDs of already loaded and visible POIs</param>
        /// <returns>list of new nearby POI locations</returns>
        public async Task<IEnumerable<Location>> Get(
            MapRectangle area,
            IEnumerable<string> visiblePoiIds)
        {
            var latLongList = GetLatLongListFromMapArea(area);

            var resultList = await this.GetPoisFromLatLongList(latLongList);

            // filter by map rectangle and already present IDs
            var visiblePoiIdSet = new HashSet<string>(visiblePoiIds);

            return resultList.Where(location =>
                !visiblePoiIdSet.Contains(location.Id) &&
                area.IsInside(location.MapLocation.Latitude, location.MapLocation.Longitude));
        }

        /// <summary>
        /// Gets a list of all lat/long combinations contained in the map rectangle.
        /// </summary>
        /// <param name="area">map area</param>
        /// <returns>
        /// list of integer latitude longitude combinations in the map area
        /// </returns>
        private static IEnumerable<LatLongKey> GetLatLongListFromMapArea(
            MapRectangle area)
        {
            int minLatitude = (int)Math.Floor(area.South);
            int maxLatitude = (int)Math.Ceiling(area.North);
            int minLongitude = (int)Math.Floor(area.West);
            int maxLongitude = (int)Math.Ceiling(area.East);

            List<LatLongKey> latLongList = new();

            for (int latitude = minLatitude; latitude < maxLatitude; latitude++)
            {
                for (int longitude = minLongitude; longitude < maxLongitude; longitude++)
                {
                    latLongList.Add(new LatLongKey(latitude, longitude));
                }
            }

            return latLongList;
        }

        /// <summary>
        /// Gets a list of locations for all the latitude/longitude combinations in the list.
        /// </summary>
        /// <param name="latLongList">lat/long list</param>
        /// <returns>list of locations</returns>
        private async Task<IEnumerable<Location>> GetPoisFromLatLongList(
            IEnumerable<LatLongKey> latLongList)
        {
            var resultList = new List<Location>();

            foreach (var (latitude, longitude) in latLongList)
            {
                var partResult = await this.FetchNearbyPoiLocations(
                    latitude,
                    longitude);

                resultList.AddRange(partResult);
            }

            return resultList;
        }

        /// <summary>
        /// Fetches more nearby POI locations in the given integer latitude and longitude area.
        /// First the local cache is checked, and if unsuccessful, the backend data service is
        /// used. May throw an exception if the network is currently not available.
        /// </summary>
        /// <param name="latitude">latitude value</param>
        /// <param name="longitude">longitude value</param>
        /// <returns>list of locations</returns>
        private async Task<IEnumerable<Location>> FetchNearbyPoiLocations(
            int latitude,
            int longitude)
        {
            var combo = new LatLongKey(latitude, longitude);

            if (this.cache.ContainsKey(combo))
            {
                return this.cache[combo];
            }

            IEnumerable<Location> result =
                await this.backendDataService.FindNearbyPoisAsync(
                    latitude,
                    longitude);

            this.cache[combo] = result.ToList();

            this.SaveCache();

            return result;
        }

        /// <summary>
        /// Loads the cache from file
        /// </summary>
        private void LoadCache()
        {
            string cacheFilename = Path.Combine(this.cacheFolder, CacheFilename);
            if (!File.Exists(cacheFilename))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(cacheFilename);

                var localCache = JsonConvert.DeserializeObject<Dictionary<LatLongKey, List<Location>>>(json);
                if (localCache != null)
                {
                    this.cache = localCache;
                }
                else
                {
                    Debug.WriteLine("Error while deserializing JSON for nearby POIs cache: " + json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while loading nearby POIs cache: " + ex.ToString());
                App.LogError(ex);
            }
        }

        /// <summary>
        /// Saves the cache to file
        /// </summary>
        private void SaveCache()
        {
            string json = JsonConvert.SerializeObject(this.cache);

            string cacheFilename = Path.Combine(this.cacheFolder, CacheFilename);
            File.WriteAllText(cacheFilename, json);
        }
    }
}
