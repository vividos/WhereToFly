using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;
using WhereToFly.WebApi.Logic;

namespace WhereToFly.WebApi.UnitTest
{
    /// <summary>
    /// Tests for LocationFindManager class
    /// </summary>
    [TestClass]
    public class LocationFindManagerTest
    {
        /// <summary>
        /// Tests initializing database by querying an unknown location
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestInitialize()
        {
            // set up
            var mgr = new LocationFindManager();

            // run
            Location location = await mgr.GetAsync("xyz123");

            // check
            Assert.IsNull(location, "location must not exist");
        }

        /// <summary>
        /// Tests timing of querying all latitude/longitude combinations
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestTimingAllLatLong()
        {
            // set up
            var mgr = new LocationFindManager();
            var stopwatch = new Stopwatch();

            // run
            ////for (int latitude = -90; latitude < 90; latitude++)
            for (int latitude = 40; latitude < 50; latitude++)
            {
                int maxNumberOfLocations = 0;
                double maxTime = 0.0;

                for (int longitude = 0; longitude < 360; longitude++)
                {
                    stopwatch.Start();

                    var results = await mgr.FindAsync(
                        new MapPoint(latitude + 0.5, longitude + 0.5),
                        rangeInMeter: 50 * 1000.0);

                    int numberOfLocations = results.Count();

                    stopwatch.Stop();

                    double time = stopwatch.Elapsed.TotalSeconds;

                    maxNumberOfLocations = Math.Max(maxNumberOfLocations, numberOfLocations);
                    maxTime = Math.Max(maxTime, time);

                    Debug.WriteLine($"{latitude}/{longitude}: {numberOfLocations} locations in {time} seconds");

                    stopwatch.Reset();
                }

                Debug.WriteLine($"{latitude}: {maxNumberOfLocations} locations in {maxTime} seconds");

                Assert.IsTrue(
                    maxNumberOfLocations > 0,
                    "there must be locations anywhere in the checked range");
            }
        }
    }
}
