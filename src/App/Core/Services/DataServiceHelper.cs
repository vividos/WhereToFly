using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Geo;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// Helper methods for data service classes
    /// </summary>
    internal static class DataServiceHelper
    {
        /// <summary>
        /// Filename of the default favicon URL cache in the Assets folder
        /// </summary>
        public const string FaviconUrlCacheFilename = "defaultFaviconUrlCache.json";

        /// <summary>
        /// Returns a default location list; used when stored location list can't be loaded
        /// </summary>
        /// <returns>default location list</returns>
        internal static List<Location> GetDefaultLocationList()
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
        /// Returns a default track list; used when no stored track list is available
        /// </summary>
        /// <returns>default track list</returns>
        internal static List<Track> GetDefaultTrackList()
        {
            return new List<Track>();
        }

        /// <summary>
        /// Returns a default mapping for the favicon cache, e.g. when the app is initialized
        /// the first time.
        /// </summary>
        /// <returns>mapping from base URL to favicon URL</returns>
        internal static Dictionary<string, string> GetDefaultFaviconCache()
        {
            var platform = DependencyService.Get<IPlatform>();

            if (!File.Exists(Path.Combine(platform.AppDataFolder, FaviconUrlCacheFilename)))
            {
                // this code path is only used in unit tests
                return new Dictionary<string, string>();
            }

            string json = platform.LoadAssetText(FaviconUrlCacheFilename);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        /// <summary>
        /// Returns the repository of all available weather icon descriptions that can be used
        /// to select weather icons for the customized list
        /// </summary>
        /// <returns>repository of all weather icons</returns>
        public static List<WeatherIconDescription> GetWeatherIconDescriptionRepository()
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

                return new List<WeatherIconDescription>();
            }
        }

        /// <summary>
        /// Checks if there is a legacy data service that can be migrated to the current data
        /// service and migrates it.
        /// </summary>
        /// <param name="dataService">current data service</param>
        /// <returns>task to wait on</returns>
        internal static async Task CheckAndMigrateDataServiceAsync(IDataService dataService)
        {
            var legacyDataService = new JsonFileDataService();
            if (!legacyDataService.AreFilesAvailable)
            {
                return;
            }

            var cancellationToken = CancellationToken.None;

            await dataService.StoreAppSettingsAsync(
                await legacyDataService.GetAppSettingsAsync(cancellationToken));

            await dataService.GetWeatherIconDescriptionDataService().AddList(
                await legacyDataService.GetWeatherIconDescriptionListAsync());

            await dataService.GetLayerDataService().AddList(
                await legacyDataService.GetLayerListAsync(cancellationToken));

            await dataService.GetLocationDataService().ClearList();
            await dataService.GetLocationDataService().AddList(
                await legacyDataService.GetLocationListAsync(cancellationToken));

            await dataService.GetTrackDataService().ClearList();
            await dataService.GetTrackDataService().AddList(
                await legacyDataService.GetTrackListAsync(cancellationToken));

            legacyDataService.Cleanup();
        }
    }
}
