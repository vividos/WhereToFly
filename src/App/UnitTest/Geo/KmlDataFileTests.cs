using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using WhereToFly.Geo.DataFormats;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests KmlDataFile class
    /// </summary>
    [TestClass]
    public class KmlDataFileTests
    {
        /// <summary>
        /// Tests getting track list, in .kmz format
        /// </summary>
        [TestMethod]
        public void TestGetTrackList()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "track_linestring.kmz");
            var kmlFile = GeoLoader.LoadGeoDataFile(filename);

            // run
            var trackList = kmlFile.GetTrackList();

            // check
            Assert.IsTrue(trackList.Any(), "track list must contain any tracks");
        }

        /// <summary>
        /// Tests loading track, in .kmz format
        /// </summary>
        [TestMethod]
        public void TestLoadTrack()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "track_linestring.kmz");

            var kmlFile = GeoLoader.LoadGeoDataFile(filename);

            // run
            var track0 = kmlFile.LoadTrack(0);
            var track1 = kmlFile.LoadTrack(1);

            // check
            Assert.IsTrue(track0.TrackPoints.Any(), "track points list must not be empty");
            Assert.IsTrue(track1.TrackPoints.Any(), "track points list must not be empty");
        }

        /// <summary>
        /// Tests loading location list, in .kml format
        /// </summary>
        [TestMethod]
        public void TestLoadLocationList()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "waypoints.kml");

            var kmlFile = GeoLoader.LoadGeoDataFile(filename);

            // run
            var locationList = kmlFile.LoadLocationList();

            // check
            Assert.IsTrue(locationList.Any(), "loaded location list must contain locations");
        }

        /// <summary>
        /// Tests loading location list, in zipped .kmz format
        /// </summary>
        [TestMethod]
        public void TestLoadLocationListKmz()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "waypoints.kmz");
            var kmlFile = GeoLoader.LoadGeoDataFile(filename);

            // run
            Assert.IsTrue(kmlFile.HasLocations(), "kml file must contain locations");
            var locationList = kmlFile.LoadLocationList();

            // check
            Assert.IsTrue(locationList.Any(), "loaded location list must contain locations");
        }

        /// <summary>
        /// Tests loading a kmz file with only tracks in it, but no placemarks
        /// </summary>
        [TestMethod]
        public void TestFileWithoutPlacemarks()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "tracks.kmz");
            var kmlFile = GeoLoader.LoadGeoDataFile(filename);

            // run
            var locationList = kmlFile.LoadLocationList();

            // check
            Assert.IsFalse(kmlFile.HasLocations(), "kml file must not contain locations");
            Assert.IsFalse(locationList.Any(), "loaded location list must not contain locations");
        }

        /// <summary>
        /// Tests loading .kml file with version kml 2.1
        /// </summary>
        [TestMethod]
        public void TestKml21File()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "kml21.kml");

            // run
            var kmlFile = GeoLoader.LoadGeoDataFile(filename);

            // check
            Assert.IsTrue(kmlFile.HasLocations(), "file must have a location");
        }
    }
}
