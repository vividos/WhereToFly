﻿using System.Text.Json;
using WhereToFly.App.Models;
using WhereToFly.App.Resources;
using WhereToFly.App.Serializers;
using WhereToFly.Geo.Airspace;
using WhereToFly.Geo.DataFormats;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Services
{
    /// <summary>
    /// Helper methods for data service classes
    /// </summary>
    internal static class DataServiceHelper
    {
        /// <summary>
        /// Filename of the default layer OpenAir file in the Assets folder
        /// </summary>
        private const string DefaultLayerFilename = "defaultLayerOpenAir.txt";

        /// <summary>
        /// Filename of the default favicon URL cache in the Assets folder
        /// </summary>
        public const string FaviconUrlCacheFilename = "defaultFaviconUrlCache.json";

        /// <summary>
        /// Returns the default layer list with only the built-in layers
        /// </summary>
        /// <returns>default layer list</returns>
        internal static IEnumerable<Layer> GetDefaultLayerList()
        {
            return new Layer[2]
            {
                new Layer("locationLayer")
                {
                    Name = "Locations",
                    IsVisible = true,
                    LayerType = LayerType.LocationLayer,
                },
                new Layer("trackLayer")
                {
                    Name = "Tracks",
                    IsVisible = true,
                    LayerType = LayerType.TrackLayer,
                },
            };
        }

        /// <summary>
        /// Returns initial layer list, containing the built-in layers and example layer(s)
        /// </summary>
        /// <returns>initial layer list</returns>
        internal static async Task<IEnumerable<Layer>> GetInitialLayerList()
        {
            return GetDefaultLayerList().Concat(await LoadExampleLayerList());
        }

        /// <summary>
        /// Loads example layer list that is used as the initial layer(s)
        /// </summary>
        /// <returns>layer list</returns>
        private static async Task<IEnumerable<Layer>> LoadExampleLayerList()
        {
            using var stream = await Assets.Get(DefaultLayerFilename);
            if (stream == null)
            {
                return Enumerable.Empty<Layer>();
            }

            var parser = new OpenAirFileParser(stream);

            const string LayerName = "OpenAir Schutzzonen Auszug";
            string czml = CzmlAirspaceWriter.WriteCzml(
                LayerName,
                parser.Airspaces,
                parser.FileCommentLines);

            string description = string.Join("\n", parser.FileCommentLines);

            var airspaceLayer = new Layer(Guid.NewGuid().ToString("B"))
            {
                Name = LayerName,
                Description = description,
                IsVisible = true,
                LayerType = LayerType.CzmlLayer,
                Data = czml,
            };

            return new Layer[]
            {
                GetOpenStreetMapBuildingsLayer(),
                airspaceLayer,
            };
        }

        /// <summary>
        /// Returns an OpenStreetMap Buildings layer
        /// </summary>
        /// <returns>newly created layer</returns>
        internal static Layer GetOpenStreetMapBuildingsLayer()
        {
            return new Layer("osmBuildingsLayer")
            {
                Name = "Cesium OpenStreetMap Buildings",
                IsVisible = true,
                LayerType = LayerType.OsmBuildingsLayer,
            };
        }

        /// <summary>
        /// Returns a default location list; used when stored location list can't be loaded
        /// </summary>
        /// <returns>default location list</returns>
        internal static List<Location> GetDefaultLocationList()
        {
            return
            [
                new Location(
                    Guid.NewGuid().ToString("B"),
                    new MapPoint(47.6764385, 11.8710533, 1685.0))
                {
                    Name = "Brecherspitz",
                    Description = "Herrliche Aussicht über die drei Seen Schliersee im Norden, Tegernsee im Westen und den Spitzingsee im Süden.",
                    Type = LocationType.Summit,
                    InternetLink = "https://de.wikipedia.org/wiki/Brecherspitz",
                },

                new Location(
                    "wheretofly-path-jagerkamp",
                    new MapPoint(47.673511, 11.9060494, 1746.0))
                {
                    Name = "Jägerkamp",
                    Description = "Gipfel in den Schlierseer Bergen, mit Ausblick auf Schliersee und die umliegenden Berge.",
                    Type = LocationType.Summit,
                    InternetLink = "https://de.wikipedia.org/wiki/J%C3%A4gerkamp",
                    IsPlanTourLocation = true,
                },

                new Location(
                    "wheretofly-path-rotwand",
                    new MapPoint(47.6502332, 11.9345058, 1884))
                {
                    Name = "Rotwand",
                    Description = "Beliebter Ausflugsgipfel.",
                    Type = LocationType.Summit,
                    InternetLink = "https://de.wikipedia.org/wiki/Rotwand_(Bayern)",
                    IsPlanTourLocation = true,
                },

                new Location(
                    Guid.NewGuid().ToString("B"),
                    new MapPoint(47.6838571, 11.8687695, 1311.0))
                {
                    Name = "Ankel-Alm",
                    Description = "Privat bewirtschaftete Alm; Montag Ruhetag",
                    Type = LocationType.AlpineHut,
                    InternetLink = string.Empty,
                },

                new Location(
                    Guid.NewGuid().ToString("B"),
                    new MapPoint(47.66508, 11.90612, 1410.0))
                {
                    Name = "Schönfeldhütte",
                    Description = "Alpenvereinshütte der Sektion München des DAV",
                    Type = LocationType.AlpineHut,
                    InternetLink = "https://www.davplus.de/huetten__wege/bewirtschaftete_huetten/uebersicht/schoenfeldhuette",
                },

                new Location(
                    "wheretofly-path-bahnhof-neuhaus",
                    new MapPoint(47.70599, 11.87451, 809.0))
                {
                    Name = "Bahnhof Neuhaus (Schliersee)",
                    Description = "Anschlussmöglichkeiten zum RBO Bus 9562 zum Spitzingsattel",
                    Type = LocationType.PublicTransportTrain,
                    InternetLink = "https://www.brb.de/strecken-fahrplaene/linie/3-munchen-hbf-holzkirchen-bayrischzell",
                    IsPlanTourLocation = true,
                },

                new Location(
                    "wheretofly-path-spitzingsattel",
                    new MapPoint(47.672138, 11.8862728, 1129.0))
                {
                    Name = "Spitzingsattel",
                    Description = "Sattel auf halbem Wege zwischen Schliersee und Spitzingsee",
                    Type = LocationType.Pass,
                    InternetLink = string.Empty,
                    IsPlanTourLocation = true,
                },

                new Location(
                    "wheretofly-path-nagelspitz-start",
                    new MapPoint(47.679201, 11.90403, 1612.0))
                {
                    Name = "SP Jägerkamp/Nagelspitz (NW-WNW, E)",
                    Description = "Hike & Fly Gelände des Drachenfliegerclub Bayrischzell. Von Josefstal (Landeplatz mit Windsack) zu Fuß in ca. 1,5 Std Aufstieg zum Jägerkamp / Nagelspitz. Startplatz liegt an der Nagelspitz. Ist auch als Startplatz für Streckenflüge in Richtung Osten (Bayrischzell - Kössen) geeignet.",
                    Type = LocationType.FlyingTakeoff,
                    InternetLink = "https://dc-bayrischzell.jimdosite.com/",
                    IsPlanTourLocation = false,
                    TakeoffDirections = TakeoffDirections.NW | TakeoffDirections.WNW | TakeoffDirections.E,
                },

                new Location(
                    "wheretofly-path-josefsthal-landeplatz",
                    new MapPoint(47.69223, 11.886778, 840.0))
                {
                    Name = "LP Josefsthal",
                    Description = "Großer Landeplatz mit Windsack ",
                    Type = LocationType.FlyingLandingPlace,
                    InternetLink = "https://dc-bayrischzell.jimdosite.com/",
                    IsPlanTourLocation = false,
                },
            ];
        }

        /// <summary>
        /// Returns a default track list; used when no stored track list is available
        /// </summary>
        /// <returns>default track list</returns>
        internal static List<Track> GetDefaultTrackList()
        {
            return [];
        }

        /// <summary>
        /// Returns a default mapping for the favicon cache, e.g. when the app is initialized
        /// the first time.
        /// </summary>
        /// <returns>mapping from base URL to favicon URL</returns>
        internal static async Task<Dictionary<string, string>> GetDefaultFaviconCache()
        {
            try
            {
                using var stream = await Assets.Get(FaviconUrlCacheFilename);
                if (stream == null)
                {
                    return [];
                }

                using var reader = new StreamReader(stream);
                string json = await reader.ReadToEndAsync();

                return JsonSerializer.Deserialize(
                    json,
                    ModelsJsonSerializerContext.Default.DictionaryStringString)
                    ?? [];
            }
            catch (Exception)
            {
                // this code path is only used in unit tests
                return [];
            }
        }

        /// <summary>
        /// Returns the repository of all available weather icon descriptions that can be used
        /// to select weather icons for the customized list
        /// </summary>
        /// <returns>repository of all weather icons</returns>
        public static async Task<IEnumerable<WeatherIconDescription>> GetWeatherIconDescriptionRepository()
        {
            try
            {
                using var stream = await Assets.Get("weathericons.json");
                if (stream == null)
                {
                    return Enumerable.Empty<WeatherIconDescription>();
                }

                using var reader = new StreamReader(stream);
                string json = await reader.ReadToEndAsync();

                var weatherIconList = JsonSerializer.Deserialize(
                    json,
                    ModelsJsonSerializerContext.Default.ListWeatherIconDescription);

                return weatherIconList ?? Enumerable.Empty<WeatherIconDescription>();
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                return new List<WeatherIconDescription>();
            }
        }
    }
}
