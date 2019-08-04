using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Geo;
using WhereToFly.App.Logic;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// Data service for the app; provides access to data storage. For now the data is stored and
    /// retrieved from the Data directory and is saved in JSON format
    /// </summary>
    public class DataService : IDataService
    {
        /// <summary>
        /// Lock for app settings json file
        /// </summary>
        private readonly object appSettingsLock = new object();

        /// <summary>
        /// Filename for the app settings json file
        /// </summary>
        private const string AppSettingsFilename = "appSettings.json";

        /// <summary>
        /// Filename for the location list json file
        /// </summary>
        private const string LocationListFilename = "locationList.json";

        /// <summary>
        /// Filename for the track list json file
        /// </summary>
        private const string TrackListFilename = "trackList.json";

        /// <summary>
        /// Filename for the layer list json file
        /// </summary>
        private const string LayerListFilename = "layerList.json";

        /// <summary>
        /// Filename for the weather icon list json file
        /// </summary>
        private const string WeatherIconListFilename = "weatherIconList.json";

        /// <summary>
        /// Filename for the favicon url cache file
        /// </summary>
        private const string FaviconUrlCacheFilename = "faviconUrlCache.json";

        /// <summary>
        /// Data service to access backend
        /// </summary>
        private readonly BackendDataService backendDataService = new BackendDataService();

        /// <summary>
        /// Cache for favicon URLs
        /// </summary>
        private readonly Dictionary<string, string> faviconUrlCache = new Dictionary<string, string>();

        /// <summary>
        /// Location list storage
        /// </summary>
        private List<Location> locationList;

        /// <summary>
        /// Track list storage
        /// </summary>
        private List<Track> trackList;

        /// <summary>
        /// Layer list storage
        /// </summary>
        private List<Layer> layerList;

        /// <summary>
        /// Loads the favicon url cache from cache data folder
        /// </summary>
        public void LoadFaviconUrlCache()
        {
            var platform = DependencyService.Get<IPlatform>();
            string cacheFilename = Path.Combine(platform.CacheDataFolder, FaviconUrlCacheFilename);

            string json = null;
            if (File.Exists(cacheFilename))
            {
                json = File.ReadAllText(cacheFilename);
            }

            if (string.IsNullOrEmpty(json) ||
                json == "{}")
            {
                json = platform.LoadAssetText("defaultFaviconUrlCache.json");
                File.WriteAllText(cacheFilename, json);
            }

            var localCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            foreach (var item in localCache)
            {
                if (!this.faviconUrlCache.ContainsKey(item.Key))
                {
                    this.faviconUrlCache.Add(item.Key, item.Value);
                }
            }
        }

        /// <summary>
        /// Stores the favicon url cache to the cache data folder
        /// </summary>
        public void StoreFaviconUrlCache()
        {
            var platform = DependencyService.Get<IPlatform>();
            string cacheFilename = Path.Combine(platform.CacheDataFolder, FaviconUrlCacheFilename);

            string json = JsonConvert.SerializeObject(this.faviconUrlCache);

            if (!string.IsNullOrEmpty(json))
            {
                File.WriteAllText(cacheFilename, json);
            }
        }

        /// <summary>
        /// Gets the current app settings object
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>app settings object</returns>
        public async Task<AppSettings> GetAppSettingsAsync(CancellationToken token)
        {
            var platform = DependencyService.Get<IPlatform>();

            string filename = Path.Combine(platform.AppDataFolder, AppSettingsFilename);

            if (!File.Exists(filename))
            {
                return new AppSettings();
            }

            try
            {
                string json;
                lock (this.appSettingsLock)
                {
                    json = File.ReadAllText(filename, Encoding.UTF8);
                }

                var appSettings = JsonConvert.DeserializeObject<AppSettings>(json);

                return await Task.FromResult(appSettings);
            }
            catch (Exception ex)
            {
                App.LogError(ex);
                return new AppSettings();
            }
        }

        /// <summary>
        /// Stores new app settings object
        /// </summary>
        /// <param name="appSettings">new app settings to store</param>
        /// <returns>task to wait on</returns>
        public async Task StoreAppSettingsAsync(AppSettings appSettings)
        {
            string json = JsonConvert.SerializeObject(appSettings);

            var platform = DependencyService.Get<IPlatform>();
            string filename = Path.Combine(platform.AppDataFolder, AppSettingsFilename);

            await Task.Run(() =>
            {
                lock (this.appSettingsLock)
                {
                    File.WriteAllText(filename, json, Encoding.UTF8);
                }
            });
        }

        /// <summary>
        /// Gets list of locations
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>list of locations</returns>
        public async Task<List<Location>> GetLocationListAsync(CancellationToken token)
        {
            if (this.locationList != null)
            {
                return this.locationList;
            }

            await Task.Run(() => this.LoadLocationList());

            return this.locationList;
        }

        /// <summary>
        /// Loads location list and stores it in the private field
        /// </summary>
        private void LoadLocationList()
        {
            var platform = DependencyService.Get<IPlatform>();

            string filename = Path.Combine(platform.AppDataFolder, LocationListFilename);

            if (!File.Exists(filename))
            {
                this.locationList = this.GetDefaultLocationList();
                return;
            }

            try
            {
                string json = File.ReadAllText(filename, Encoding.UTF8);

                this.locationList = JsonConvert.DeserializeObject<List<Location>>(json);
            }
            catch (Exception ex)
            {
                App.LogError(ex);
                this.locationList = this.GetDefaultLocationList();
            }
        }

        /// <summary>
        /// Returns a default location list; used when stored location list can't be loaded
        /// </summary>
        /// <returns>default location list</returns>
        private List<Location> GetDefaultLocationList()
        {
            return new List<Location>
            {
                new Location
                {
                    Id = Guid.NewGuid().ToString("B"),
                    Name = "Brecherspitz",
                    MapLocation = new MapPoint(47.6764385, 11.8710533, 1685.0),
                    Description = "Herrliche Aussicht über die drei Seen Schliersee im Norden, Tegernsee im Westen und den Spitzingsee im Süden.",
                    Type = LocationType.Summit,
                    InternetLink = "https://de.wikipedia.org/wiki/Brecherspitz"
                },

                new Location
                {
                    Id = "wheretofly-path-jagerkamp",
                    Name = "Jägerkamp",
                    MapLocation = new MapPoint(47.673511, 11.9060494, 1746.0),
                    Description = "Gipfel in den Schlierseer Bergen, mit Ausblick auf Schliersee und die umliegenden Berge.",
                    Type = LocationType.Summit,
                    InternetLink = "https://de.wikipedia.org/wiki/J%C3%A4gerkamp",
                    IsPlanTourLocation = true,
                },

                new Location
                {
                    Id = "wheretofly-path-rotwand",
                    Name = "Rotwand",
                    MapLocation = new MapPoint(47.6502332, 11.9345058, 1884),
                    Description = "Beliebter Ausflugsgipfel.",
                    Type = LocationType.Summit,
                    InternetLink = "https://de.wikipedia.org/wiki/Rotwand_(Bayern)",
                    IsPlanTourLocation = true,
                },

                new Location
                {
                    Id = Guid.NewGuid().ToString("B"),
                    Name = "Ankel-Alm",
                    MapLocation = new MapPoint(47.6838571, 11.8687695, 1311.0),
                    Description = "Privat bewirtschaftete Alm; Montag Ruhetag",
                    Type = LocationType.AlpineHut,
                    InternetLink = string.Empty
                },

                new Location
                {
                    Id = Guid.NewGuid().ToString("B"),
                    Name = "Schönfeldhütte",
                    MapLocation = new MapPoint(47.66508, 11.90612, 1410.0),
                    Description = "Alpenvereinshütte der Sektion München des DAV",
                    Type = LocationType.AlpineHut,
                    InternetLink = "https://www.davplus.de/huetten__wege/bewirtschaftete_huetten/uebersicht/schoenfeldhuette"
                },

                new Location
                {
                    Id = "wheretofly-path-bahnhof-neuhaus",
                    Name = "Bahnhof Neuhaus (Schliersee)",
                    MapLocation = new MapPoint(47.70599, 11.87451, 809.0),
                    Description = "Anschlussmöglichkeiten zum RBO Bus 9562 zum Spitzingsattel",
                    Type = LocationType.PublicTransportTrain,
                    InternetLink = "http://www.bayerischeoberlandbahn.de/strecken-fahrplaene/linie/3-munchen-hbf-holzkirchen-bayrischzell",
                    IsPlanTourLocation = true
                },

                new Location
                {
                    Id = Guid.NewGuid().ToString("B"),
                    Name = "Spitzingsattel",
                    MapLocation = new MapPoint(47.672138, 11.8862728, 1129.0),
                    Description = "Sattel auf halbem Wege zwischen Schliersee und Spitzingsee",
                    Type = LocationType.Pass,
                    InternetLink = string.Empty,
                    IsPlanTourLocation = true
                },
            };
        }

        /// <summary>
        /// Stores new location list
        /// </summary>
        /// <param name="locationList">location list to store</param>
        /// <returns>task to wait on</returns>
        public async Task StoreLocationListAsync(List<Location> locationList)
        {
            this.locationList = locationList;

            string json = JsonConvert.SerializeObject(locationList);

            var platform = DependencyService.Get<IPlatform>();
            string filename = Path.Combine(platform.AppDataFolder, LocationListFilename);

            await Task.Run(() => File.WriteAllText(filename, json, Encoding.UTF8));
        }

        /// <summary>
        /// Gets list of tracks
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>list of tracks</returns>
        public async Task<List<Track>> GetTrackListAsync(CancellationToken token)
        {
            if (this.trackList != null)
            {
                return this.trackList;
            }

            await Task.Run(() => this.LoadTrackList());

            return await Task.FromResult(this.trackList);
        }

        /// <summary>
        /// Loads track list and stores it in the private field
        /// </summary>
        private void LoadTrackList()
        {
            var platform = DependencyService.Get<IPlatform>();

            string filename = Path.Combine(platform.AppDataFolder, TrackListFilename);

            if (!File.Exists(filename))
            {
                this.trackList = GetDefaultTrackList();
                return;
            }

            try
            {
                string json = File.ReadAllText(filename, Encoding.UTF8);

                this.trackList = JsonConvert.DeserializeObject<List<Track>>(json);
            }
            catch (Exception ex)
            {
                App.LogError(ex);
                this.trackList = new List<Track>();
            }
        }

        /// <summary>
        /// Returns a default track list; used when no stored track list is available
        /// </summary>
        /// <returns>default track list</returns>
        private static List<Track> GetDefaultTrackList()
        {
            return new List<Track>();
        }

        /// <summary>
        /// Stores new track list
        /// </summary>
        /// <param name="trackList">track list to store</param>
        /// <returns>task to wait on</returns>
        public async Task StoreTrackListAsync(List<Track> trackList)
        {
            this.trackList = trackList;

            string json = JsonConvert.SerializeObject(trackList);

            var platform = DependencyService.Get<IPlatform>();
            string filename = Path.Combine(platform.AppDataFolder, TrackListFilename);

            await Task.Run(() => File.WriteAllText(filename, json, Encoding.UTF8));
        }

        /// <summary>
        /// Gets list of layers
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>list of layers</returns>
        public async Task<List<Layer>> GetLayerListAsync(CancellationToken token)
        {
            if (this.layerList != null)
            {
                return this.layerList;
            }

            await Task.Run(() => this.LoadLayerList());

            return await Task.FromResult(this.layerList);
        }

        /// <summary>
        /// Loads layer list and stores it in the private field
        /// </summary>
        private void LoadLayerList()
        {
            var platform = DependencyService.Get<IPlatform>();

            string filename = Path.Combine(platform.AppDataFolder, LayerListFilename);

            if (!File.Exists(filename))
            {
                this.layerList = new List<Layer>();
                return;
            }

            try
            {
                string json = File.ReadAllText(filename, Encoding.UTF8);

                this.layerList = JsonConvert.DeserializeObject<List<Layer>>(json);
            }
            catch (Exception ex)
            {
                App.LogError(ex);
                this.layerList = new List<Layer>();
            }
        }

        /// <summary>
        /// Stores new layer list
        /// </summary>
        /// <param name="layerList">layer list to store</param>
        /// <returns>task to wait on</returns>
        public async Task StoreLayerListAsync(List<Layer> layerList)
        {
            this.layerList = layerList;

            string json = JsonConvert.SerializeObject(layerList);

            var platform = DependencyService.Get<IPlatform>();
            string filename = Path.Combine(platform.AppDataFolder, LayerListFilename);

            await Task.Run(() => File.WriteAllText(filename, json, Encoding.UTF8));
        }

        /// <summary>
        /// Retrieves list of weather icon descriptions
        /// </summary>
        /// <returns>list with current weather icon descriptions</returns>
        public async Task<List<WeatherIconDescription>> GetWeatherIconDescriptionListAsync()
        {
            var platform = DependencyService.Get<IPlatform>();

            string filename = Path.Combine(platform.AppDataFolder, WeatherIconListFilename);

            if (!File.Exists(filename))
            {
                return await Task.FromResult(new List<WeatherIconDescription>());
            }

            try
            {
                string json = File.ReadAllText(filename, Encoding.UTF8);

                var weatherIconList = JsonConvert.DeserializeObject<List<WeatherIconDescription>>(json);

                return await Task.FromResult(weatherIconList);
            }
            catch (Exception ex)
            {
                App.LogError(ex);
                return await Task.FromResult(new List<WeatherIconDescription>());
            }
        }

        /// <summary>
        /// Stores new weather icon list
        /// </summary>
        /// <param name="weatherIconList">weather icon list to store</param>
        /// <returns>task to wait on</returns>
        public async Task StoreWeatherIconDescriptionListAsync(List<WeatherIconDescription> weatherIconList)
        {
            string json = JsonConvert.SerializeObject(weatherIconList);

            var platform = DependencyService.Get<IPlatform>();
            string filename = Path.Combine(platform.AppDataFolder, WeatherIconListFilename);

            await Task.Run(() => File.WriteAllText(filename, json, Encoding.UTF8));
        }

        /// <summary>
        /// Returns the repository of all available weather icon descriptions that can be used
        /// to select weather icons for the customized list
        /// </summary>
        /// <returns>repository of all weather icons</returns>
        public List<WeatherIconDescription> GetWeatherIconDescriptionRepository()
        {
            try
            {
                var platform = DependencyService.Get<IPlatform>();

                string json = platform.LoadAssetText("weathericons.json");

                var weatherIconList = JsonConvert.DeserializeObject<List<WeatherIconDescription>>(json);
                return weatherIconList;
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                return new List<WeatherIconDescription>
                {
                    new WeatherIconDescription
                    {
                        Name = "Add new...",
                        Type = WeatherIconDescription.IconType.IconPlaceholder,
                    },
                };
            }
        }

        /// <summary>
        /// Retrieves a favicon URL for the given website URL
        /// </summary>
        /// <param name="websiteUrl">website URL</param>
        /// <returns>favicon URL or empty string when none was found</returns>
        public async Task<string> GetFaviconUrlAsync(string websiteUrl)
        {
            var uri = new Uri(websiteUrl);
            string baseUri = $"{uri.Scheme}://{uri.Host}/";

            if (uri.Host.ToLowerInvariant() == "localhost")
            {
                return $"{uri.Scheme}://{uri.Host}/favicon.ico";
            }

            if (this.faviconUrlCache.ContainsKey(baseUri))
            {
                return this.faviconUrlCache[baseUri];
            }

            try
            {
                string faviconUrl = await this.backendDataService.GetFaviconUrlAsync(websiteUrl);

                this.faviconUrlCache[baseUri] = faviconUrl;

                return faviconUrl;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Retrieves latest info about a live waypoint, including new coordinates and
        /// description.
        /// </summary>
        /// <param name="liveWaypointId">live waypoint ID</param>
        /// <returns>query result for live waypoint</returns>
        public async Task<LiveWaypointQueryResult> GetLiveWaypointDataAsync(string liveWaypointId)
        {
            return await this.backendDataService.GetLiveWaypointDataAsync(liveWaypointId);
        }

        /// <summary>
        /// Plans a tour with given tour planning parameters and returns the planned tour.
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        /// <returns>planned tour</returns>
        public async Task<PlannedTour> PlanTourAsync(PlanTourParameters planTourParameters)
        {
            return await this.backendDataService.PlanTourAsync(planTourParameters);
        }
    }
}
