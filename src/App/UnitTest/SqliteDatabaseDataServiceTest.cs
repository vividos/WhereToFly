using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.Services.SqliteDatabase;
using WhereToFly.App.Geo;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for class SqliteDatabaseDataService
    /// </summary>
    [TestClass]
    public class SqliteDatabaseDataServiceTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IPlatform, UnitTestPlatform>();

            // start with a new database
            var platform = DependencyService.Get<IPlatform>();
            string databaseFilename = Path.Combine(platform.AppDataFolder, "database.db");

            if (File.Exists(databaseFilename))
            {
                File.Delete(databaseFilename);
            }
        }

        /// <summary>
        /// Returns AppSettings object for tests
        /// </summary>
        /// <returns>app settings object</returns>
        private static AppSettings GetTestAppSettings()
        {
            return new AppSettings
            {
                MapImageryType = MapImageryType.BingMapsAerialWithLabels,
                MapOverlayType = MapOverlayType.ThermalSkywaysKk7,
                ShadingMode = MapShadingMode.Fixed10Am,
                CoordinateDisplayFormat = CoordinateDisplayFormat.Format_dd_dddddd,
                LastLocationListFilterText = "Brecherspitz",
                LastShownPosition = new MapPoint(47.6764385, 11.8710533, 1685.0),
                LastFlyingRangeParameters = new FlyingRangeParameters
                {
                    GliderSpeed = 35,
                    GlideRatio = 9.0,
                    WindDirection = 270,
                    WindSpeed = 15.0
                }
            };
        }

        /// <summary>
        /// Tests getting default AppSettings object from freshly initialized Sqlite database data
        /// service
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestDefaultAppSettings()
        {
            // set up
            var service = new SqliteDatabaseDataService();

            // run
            var appSettings = await service.GetAppSettingsAsync(CancellationToken.None);

            // check
            Assert.IsNotNull(appSettings, "app settings object must be available");
        }

        [TestMethod]
        public async Task TestStoreAndGetAppSettings()
        {
            // set up
            var service = new SqliteDatabaseDataService();
            var appSettings = GetTestAppSettings();

            // run
            await service.StoreAppSettingsAsync(appSettings);
            var appSettings2 = await service.GetAppSettingsAsync(CancellationToken.None);

            // check
            Assert.IsNotNull(appSettings2, "loaded app settings object must be non-null");
            Assert.AreEqual(appSettings, appSettings2, "stored app settings object must match initial one");
        }

        /// <summary>
        /// Tests migrating data from JsonFileDataService
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestMigrateFromJsonFileDataService()
        {
            // set up
            var legacyService = new JsonFileDataService();

            AppSettings appSettings = GetTestAppSettings();
            await legacyService.StoreAppSettingsAsync(appSettings);

            var myTrackList = new List<Track>
            {
                new Track
                {
                    Id = "my-track",
                    Name = "My Track",
                    IsFlightTrack = false,
                    TrackPoints = new List<TrackPoint>
                    {
                        new TrackPoint(47.754076, 12.352277, 1234.0, null)
                        {
                            Time = DateTime.Today + TimeSpan.FromHours(1.0)
                        },
                        new TrackPoint(47.754076, 12.352277, null, 270),
                    }
                }
            };
            await legacyService.StoreTrackListAsync(myTrackList);

            Assert.IsTrue(legacyService.AreFilesAvailable, "legacy files must be available");

            // run
            var dataService = new SqliteDatabaseDataService();
            await DataServiceHelper.CheckAndMigrateDataServiceAsync(dataService);

            // check
            Assert.IsFalse(legacyService.AreFilesAvailable, "legacy files must not be available");

            var migratedAppSettings = await dataService.GetAppSettingsAsync(CancellationToken.None);

            Assert.IsNotNull(migratedAppSettings, "loaded app settings object must be non-null");
            Assert.AreEqual(appSettings, migratedAppSettings, "migrated app settings must match initial one");

            var migratedTrackList = (await dataService.GetTrackDataService().GetList()).ToList();

            Assert.AreEqual(myTrackList.Count, migratedTrackList.Count, "track list length must match");
            Assert.AreEqual(myTrackList[0].Id, migratedTrackList[0].Id, "migrated track ID must match");
            Assert.AreEqual(myTrackList[0].Name, migratedTrackList[0].Name, "migrated track name must match");
            Assert.AreEqual(
                myTrackList[0].TrackPoints.Count,
                migratedTrackList[0].TrackPoints.Count,
                "migrated track point list length must match");
        }
    }
}
