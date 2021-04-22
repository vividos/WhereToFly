using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using WhereToFly.Geo.DataFormats;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests for SeeYouDataFile class
    /// </summary>
    [TestClass]
    public class SeeYouDataFileTest
    {
        /// <summary>
        /// Tests method LoadLocations(), loading an empty .cup file
        /// </summary>
        [TestMethod]
        public void TestLoadLocationList_EmptyWaypointFile()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "waypoints-empty.cup");
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                // run
                var cupFile = new SeeYouDataFile(stream);

                // check
                Assert.IsFalse(cupFile.HasLocations(), "loaded file must have no locations");
                Assert.IsFalse(cupFile.LoadLocationList().Any(), "locations list must contain no locations");
            }
        }

        /// <summary>
        /// Tests method LoadLocations(), loading a .cup file with variants of locations
        /// </summary>
        [TestMethod]
        public void TestLoadLocationList_WaypointVariants()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "waypoints-variants.cup");
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                // run
                var cupFile = new SeeYouDataFile(stream);

                var locationsList = cupFile.LoadLocationList();

                // check
                Assert.IsTrue(cupFile.HasLocations(), "loaded file must have locations");
                Assert.AreEqual(4, locationsList.Count, "locations list must contain correct number of locations");
            }
        }
    }
}
