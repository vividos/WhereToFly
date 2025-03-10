﻿using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.MapView;
using WhereToFly.App.Models;
using WhereToFly.App.Services.SqliteDatabase;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for class SqliteDatabaseDataService
    /// </summary>
    [TestClass]
    public class SqliteDatabaseDataServiceTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            // start with a new database
            string? folder = Path.GetDirectoryName(this.GetType().Assembly.Location);
            Assert.IsNotNull(folder, "test folder must be available");

            string databaseFilename = Path.Combine(
                folder,
                "database.db");

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
                LastLocationFilterSettings = new LocationFilterSettings
                {
                    FilterText = "Brecherspitz",
                },
                LastShownPosition = new MapPoint(47.6764385, 11.8710533, 1685.0),
                LastFlyingRangeParameters = new FlyingRangeParameters
                {
                    GliderSpeed = 35,
                    GlideRatio = 9.0,
                    WindDirection = 270,
                    WindSpeed = 15.0,
                    AltitudeOffset = 50,
                },
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

            await service.CloseAsync();
        }

        /// <summary>
        /// Tests storing and getting AppSettings object from the database service
        /// </summary>
        /// <returns>task to wait on</returns>
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

            await service.CloseAsync();
        }
    }
}
