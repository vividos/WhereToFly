using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.WebApi.Logic
{
    /// <summary>
    /// Location find manager; lets the user find locations nearby.
    /// The manager uses a memory database to load locations and to accelerate searches.
    /// </summary>
    public class LocationFindManager
    {
        /// <summary>
        /// Maximum radius to query locations
        /// </summary>
        private const double MaximumRadiusInMeter = 50 * 1000;

        /// <summary>
        /// Task that is completed when the manager is initialized completely.
        /// </summary>
        private readonly Task initializedTask;

        /// <summary>
        /// Memory database connection
        /// </summary>
        private SQLiteAsyncConnection connection;

        /// <summary>
        /// Creates a new location find manager
        /// </summary>
        public LocationFindManager()
        {
            this.initializedTask = this.InitAsync();
        }

        /// <summary>
        /// Initializes the manager
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task InitAsync()
        {
            this.connection = new SQLiteAsyncConnection(
                ":memory:",
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

            var result = await this.connection.CreateTableAsync<FindLocationEntry>();

            if (result == CreateTableResult.Created)
            {
                try
                {
                    await this.ImportDataAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Imports all location data into the database for faster searching
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportDataAsync()
        {
            Tuple<string, string>[] importAndPrefixList = new Tuple<string, string>[]
            {
                new Tuple<string, string>("paraglidingspots-complete.kmz", "pgspots"),
            };

            var logicAssembly = typeof(LocationFindManager).Assembly;
            string namespaceName = typeof(LocationFindManager).Namespace;

            foreach (var importAndPrefix in importAndPrefixList)
            {
                string importFilename = importAndPrefix.Item1;
                string idPrefix = importAndPrefix.Item2;

                var kmlStream = logicAssembly.GetManifestResourceStream(
                    $"{namespaceName}.Assets.{importFilename}");

                bool isKml = Path.GetExtension(importFilename).ToLowerInvariant() == "kml";

                var importer = new KmlLocationImporter(
                    idPrefix,
                    kmlStream,
                    isKml);

                var locationList = await importer.ImportAsync();

                var entriesList = locationList.Select(location => new FindLocationEntry(location));

                await this.connection.InsertAllAsync(entriesList);
            }
        }

        /// <summary>
        /// Returns a location with given ID
        /// </summary>
        /// <param name="locationId">location ID</param>
        /// <returns>location object</returns>
        public async Task<Location> GetAsync(string locationId)
        {
            await this.initializedTask;

            var layerEntry = await this.connection.FindAsync<FindLocationEntry>(locationId);

            return layerEntry?.Location;
        }

        /// <summary>
        /// Finds all locations around a specific map point
        /// </summary>
        /// <param name="mapPoint">map point</param>
        /// <param name="rangeInMeter">range in meter around the point</param>
        /// <returns>list of locations found</returns>
        public async Task<IEnumerable<Location>> FindAsync(MapPoint mapPoint, double rangeInMeter)
        {
            await this.initializedTask;

            MapPoint minMapPoint = mapPoint.Offset(rangeInMeter, -rangeInMeter, 0.0);
            MapPoint maxMapPoint = mapPoint.Offset(-rangeInMeter, rangeInMeter, 0.0);

            var result = await this.connection.QueryAsync<FindLocationEntry>(
                "select * from locations where latitude >= ? and latitude <= ? and longitude >= ? and longitude <= ?",
                minMapPoint.Latitude,
                maxMapPoint.Latitude,
                minMapPoint.Longitude,
                maxMapPoint.Longitude);

            // since the query yields a rectangle, filter by actual distance
            rangeInMeter = Math.Min(rangeInMeter, MaximumRadiusInMeter);

            return result
                .Where(entry => FilterLocationByRange(entry.Location, mapPoint, rangeInMeter))
                .Select(entry => entry.Location);
        }

        /// <summary>
        /// Determines if a location is inside a circle with given center point and radius
        /// </summary>
        /// <param name="locationToCheck">location to check</param>
        /// <param name="centerPoint">center point of circle</param>
        /// <param name="radiusInMeter">circle radius in meter</param>
        /// <returns>true when the point is inside the circle, or false when not</returns>
        private static bool FilterLocationByRange(
            Location locationToCheck,
            MapPoint centerPoint,
            double radiusInMeter)
        {
            return centerPoint.DistanceTo(locationToCheck.MapLocation) < radiusInMeter;
        }
    }
}
