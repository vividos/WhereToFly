using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core.Services
{
    /// <summary>
    /// Data service for the app; provides access to data storage. For now the data is stored and
    /// retrieved from the Data directory and is saved in JSON format
    /// </summary>
    public class DataService
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
            catch (Exception)
            {
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

            await Task.Factory.StartNew(() => File.WriteAllText(filename, json, Encoding.UTF8));
        }

        /// <summary>
        /// Gets list of locations
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>list of locations</returns>
        public async Task<List<Location>> GetLocationListAsync(CancellationToken token)
        {
            var platform = DependencyService.Get<IPlatform>();

            string filename = Path.Combine(platform.AppDataFolder, LocationListFilename);

            if (!File.Exists(filename))
            {
                return this.GetDefaultLocationList();
            }

            try
            {
                string json = File.ReadAllText(filename, Encoding.UTF8);

                var locationList = JsonConvert.DeserializeObject<List<Location>>(json);

                return await Task.FromResult(locationList);
            }
            catch (Exception)
            {
                return this.GetDefaultLocationList();
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
                }
            };
        }

        /// <summary>
        /// Stores new location list
        /// </summary>
        /// <param name="locationList">location list to store</param>
        /// <returns>task to wait on</returns>
        public async Task StoreLocationListAsync(List<Location> locationList)
        {
            string json = JsonConvert.SerializeObject(locationList);

            var platform = DependencyService.Get<IPlatform>();
            string filename = Path.Combine(platform.AppDataFolder, LocationListFilename);

            await Task.Factory.StartNew(() => File.WriteAllText(filename, json, Encoding.UTF8));
        }
    }
}
