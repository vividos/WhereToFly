using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WhereToFly.Shared.Model;
using WhereToFly.WebApi.Logic.Services;

namespace WhereToFly.WebApi.UnitTest
{
    /// <summary>
    /// Tests for the Garmin inREach data service class
    /// </summary>
    [TestClass]
    public class GarminInreachTest
    {
        /// <summary>
        /// Returns the Assets path for all unit tests; place your test files in the Assets folder
        /// and mark them with "Content" and "Copy if newer".
        /// </summary>
        public string TestAssetsPath
        {
            get
            {
                return Path.Combine(
                    Path.GetDirectoryName(this.GetType().Assembly.Location),
                    "Assets");
            }
        }

        /// <summary>
        /// Test to get live waypoint data using data service
        /// </summary>
        [TestMethod]
        public void TestLiveWaypointData()
        {
            // run
            string filename = Path.Combine(this.TestAssetsPath, "GarminInreachRawKmlData.kml");

            LiveWaypointData data = null;
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var dataService = new GarminInreachDataService();

                data = dataService.ParseRawKmlFileWaypointData(stream, "test123");
            }

            // check
            Assert.IsNotNull(data, "returned data must not be null");
            Assert.IsTrue(data.ID.Length > 0, "ID must be non-empty");
            Assert.IsTrue(data.Name.Length > 0, "name must be non-empty");
            Assert.IsTrue(data.Description.Length > 0, "description must be non-empty");
        }


        /// <summary>
        /// Test to get live track data using data service
        /// </summary>
        [TestMethod]
        public void TestLiveTrackData()
        {
            // set up
            string filename = Path.Combine(this.TestAssetsPath, "GarminInreachRawKmlTrackData.kml");

            using var stream = new FileStream(filename, FileMode.Open);

            // run
            var dataService = new GarminInreachDataService();

            LiveTrackData data = dataService.ParseRawKmlFileTrackData(stream, "test123");

            // check
            Assert.IsNotNull(data, "returned data must not be null");
            Assert.IsTrue(data.ID.Length > 0, "ID must be non-empty");
            Assert.IsTrue(data.Name.Length > 0, "name must be non-empty");
            Assert.IsTrue(data.Description.Length > 0, "description must be non-empty");
        }
    }
}
