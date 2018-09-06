using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using WhereToFly.App.Geo.DataFormats;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests GeoLoader class
    /// </summary>
    [TestClass]
    public class GeoLoaderTest
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
        /// Tests getting track list, in .kmz format
        /// </summary>
        [TestMethod]
        public void TestGetTrackListKmz()
        {
            // run
            string filename = Path.Combine(this.TestAssetsPath, "track_linestring.kmz");
            var trackList = GeoLoader.GetTrackList(filename);

            // check
            Assert.IsTrue(trackList.Any(), "track list must contain any tracks");
        }

        /// <summary>
        /// Tests loading track, in .kmz format
        /// </summary>
        [TestMethod]
        public void TestLoadTrackKmz()
        {
            // run
            string filename = Path.Combine(this.TestAssetsPath, "track_linestring.kmz");
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var track0 = GeoLoader.LoadTrack(stream, filename, 0);
                stream.Seek(0, SeekOrigin.Begin);
                var track1 = GeoLoader.LoadTrack(stream, filename, 1);

                // check
                Assert.IsTrue(track0.TrackPoints.Any(), "track points list must not be empty");
                Assert.IsTrue(track1.TrackPoints.Any(), "track points list must not be empty");
            }
        }

        /// <summary>
        /// Tests loading location list, in .kml format
        /// </summary>
        [TestMethod]
        public void TestLoadLocationListKml()
        {
            // run
            string filename = Path.Combine(this.TestAssetsPath, "waypoints.kml");
            var locationList = GeoLoader.LoadLocationList(filename);

            // check
            Assert.IsTrue(locationList.Any(), "loaded location list must contain locations");
        }

        /// <summary>
        /// Tests loading location list, in zipped .kmz format
        /// </summary>
        [TestMethod]
        public void TestLoadLocationListKmz()
        {
            // run
            string filename = Path.Combine(this.TestAssetsPath, "waypoints.kmz");
            var locationList = GeoLoader.LoadLocationList(filename);

            // check
            Assert.IsTrue(locationList.Any(), "loaded location list must contain locations");
        }

        /// <summary>
        /// Tests getting track list, in .gpx format
        /// </summary>
        [TestMethod]
        public void TestGetTrackListGpx()
        {
            // run
            string filename = Path.Combine(this.TestAssetsPath, "tracks.gpx");
            var trackList = GeoLoader.GetTrackList(filename);

            // check
            Assert.IsTrue(trackList.Any(), "track list list must at least one track");
        }

        /// <summary>
        /// Tests loading track, in .gpx format
        /// </summary>
        [TestMethod]
        public void TestLoadTrackGpx()
        {
            // run
            string filename = Path.Combine(this.TestAssetsPath, "tracks.gpx");
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var track = GeoLoader.LoadTrack(stream, filename, 0);

                // check
                Assert.IsTrue(track.TrackPoints.Any(), "track points list must not be empty");
            }
        }

        /// <summary>
        /// Tests loading location list, in .gpx format
        /// </summary>
        [TestMethod]
        public void TestLoadLocationListGpx()
        {
            // run
            string filename = Path.Combine(this.TestAssetsPath, "waypoints.gpx");
            var locationList = GeoLoader.LoadLocationList(filename);

            // check
            Assert.IsTrue(locationList.Any(), "loaded location list must contain locations");
        }

        /// <summary>
        /// Tests loading a kmz file with only tracks in it, but no placemarks
        /// </summary>
        [TestMethod]
        public void TestTrackKmzWithoutPlacemarksKmz()
        {
            // run
            string filename = Path.Combine(this.TestAssetsPath, "tracks.kmz");
            var locationList = GeoLoader.LoadLocationList(filename);

            // check
            Assert.IsFalse(locationList.Any(), "loaded location list must not contain locations");
        }

        /// <summary>
        /// Tests loading location list, with non-existent file
        /// </summary>
        [TestMethod]
        public void TestLoadLocationList_NonExistentFile()
        {
            // set up
            string filename = Path.Combine(this.TestAssetsPath, "waypoints.abc");

            // run + check
            Assert.ThrowsException<FileNotFoundException>(() => GeoLoader.LoadLocationList(filename), "must throw exception");
        }

        /// <summary>
        /// Tests loading location list, with invalid file extension
        /// </summary>
        [TestMethod]
        public void TestLoadLocationList_InvalidFileExtension()
        {
            // set up
            string filename = "waypoints.abc";
            var stream = new MemoryStream(new byte[] { 42 });

            // run + check
            Assert.ThrowsException<ArgumentException>(() => GeoLoader.LoadLocationList(stream, filename), "must throw exception");
        }
    }
}
