using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Geo;
using WhereToFly.App.Model;
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
        /// Filename for the weather icon list json file
        /// </summary>
        private const string WeatherIconListFilename = "weatherIconList.json";

        /// <summary>
        /// Location list storage
        /// </summary>
        private List<Location> locationList;

        /// <summary>
        /// Track list storage
        /// </summary>
        private List<Track> trackList;

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
                string json = File.ReadAllText(filename, Encoding.UTF8);

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

            await Task.Run(() => File.WriteAllText(filename, json, Encoding.UTF8));
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
                    Elevation = 1685,
                    MapLocation = new MapPoint(47.6764385, 11.8710533),
                    Description = "Herrliche Aussicht über die drei Seen Schliersee im Norden, Tegernsee im Westen und den Spitzingsee im Süden.",
                    Type = LocationType.Summit,
                    InternetLink = "https://de.wikipedia.org/wiki/Brecherspitz"
                },

                new Location
                {
                    Id = Guid.NewGuid().ToString("B"),
                    Name = "Jägerkamp",
                    Elevation = 1746,
                    MapLocation = new MapPoint(47.673511, 11.9060494),
                    Description = "Gipfel in den Schlierseer Bergen, mit Ausblick auf Schliersee und die umliegenden Berge.",
                    Type = LocationType.Summit,
                    InternetLink = "https://de.wikipedia.org/wiki/J%C3%A4gerkamp"
                },

                new Location
                {
                    Id = Guid.NewGuid().ToString("B"),
                    Name = "Ankel-Alm",
                    Elevation = 1311,
                    MapLocation = new MapPoint(47.6838571, 11.8687695),
                    Description = "Privat bewirtschaftete Alm; Montag Ruhetag",
                    Type = LocationType.AlpineHut,
                    InternetLink = string.Empty
                },

                new Location
                {
                    Id = Guid.NewGuid().ToString("B"),
                    Name = "Schönfeldhütte",
                    Elevation = 1410,
                    MapLocation = new MapPoint(47.66508, 11.90612),
                    Description = "Alpenvereinshütte der Sektion München des DAV",
                    Type = LocationType.AlpineHut,
                    InternetLink = "https://www.davplus.de/huetten__wege/bewirtschaftete_huetten/uebersicht/schoenfeldhuette"
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
            return new List<Track>
            {
                new Track
                {
                    Id = "crossingthealps2018",
                    Name = "Crossing the Alps 2018",
                    IsFlightTrack = false,
                    Color = "FF0000",
                    TrackPoints = new List<TrackPoint>
                        {
                            new TrackPoint(47.754076, 12.352277, null, null), // Kampenwand
                            new TrackPoint(47.631745, 12.431815, null, null), // Kössen
                            new TrackPoint(47.285720, 12.297016, null, null), // Wildkogel
                            new TrackPoint(47.090525, 12.183008, null, null), // Alpenhauptkamm
                            new TrackPoint(46.738669, 11.958434, null, null), // Kronplatz
                            new TrackPoint(46.508371, 11.828376, null, null), // Sellastock
                            new TrackPoint(46.251668, 11.870709, null, null), // Pala
                            new TrackPoint(46.017779, 11.900711, null, null), // Feltre
                        }
                }
            };
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
    }
}
