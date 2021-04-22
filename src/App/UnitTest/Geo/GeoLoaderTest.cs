using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using WhereToFly.Geo.DataFormats;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests GeoLoader class
    /// </summary>
    [TestClass]
    public class GeoLoaderTest
    {
        /// <summary>
        /// Tests method GeoLoader.LoadGeoDataFile() to load .kml file
        /// </summary>
        [TestMethod]
        public void TestLoadGetDataFileKml()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "waypoints.kml");

            // run
            var kmlFile = GeoLoader.LoadGeoDataFile(filename);

            // check
            Assert.IsNotNull(kmlFile, "loaded geo data file must not be null");
        }

        /// <summary>
        /// Tests method GeoLoader.LoadGeoDataFile() to load .kmz file
        /// </summary>
        [TestMethod]
        public void TestLoadGetDataFileKmz()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "track_linestring.kmz");

            // run
            var kmlFile = GeoLoader.LoadGeoDataFile(filename);

            // check
            Assert.IsNotNull(kmlFile, "loaded geo data file must not be null");
        }

        /// <summary>
        /// Tests method GeoLoader.LoadGeoDataFile() to load .gpx file
        /// </summary>
        [TestMethod]
        public void TestLoadGetDataFileGpx()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "tracks.gpx");

            // run
            var gpxFile = GeoLoader.LoadGeoDataFile(filename);

            // check
            Assert.IsNotNull(gpxFile, "loaded geo data file must not be null");
        }

        /// <summary>
        /// Tests method GeoLoader.LoadGeoDataFile() to load .igc file
        /// </summary>
        [TestMethod]
        public void TestLoadGetDataFileIgc()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "85QA3ET1.igc");

            // run
            var igcFile = GeoLoader.LoadGeoDataFile(filename);

            // check
            Assert.IsNotNull(igcFile, "loaded geo data file must not be null");
        }

        /// <summary>
        /// Tests method GeoLoader.LoadGeoDataFile() to load .cup file
        /// </summary>
        [TestMethod]
        public void TestLoadGetDataFileCup()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "waypoints-variants.cup");

            // run
            var cupFile = GeoLoader.LoadGeoDataFile(filename);

            // check
            Assert.IsNotNull(cupFile, "loaded geo data file must not be null");
        }

        /// <summary>
        /// Tests loading geo data file, with non-existent file
        /// </summary>
        [TestMethod]
        public void TestLoadGeoDataFile_NonExistentFile()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "waypoints.abc");

            // run + check
            Assert.ThrowsException<FileNotFoundException>(() => GeoLoader.LoadGeoDataFile(filename), "must throw exception");
        }

        /// <summary>
        /// Tests loading geo data file, with invalid file extension
        /// </summary>
        [TestMethod]
        public void TestLoadGeoDataFile_InvalidFileExtension()
        {
            // set up
            string filename = "waypoints.abc";
            var stream = new MemoryStream(new byte[] { 42 });

            // run + check
            Assert.ThrowsException<ArgumentException>(() => GeoLoader.LoadGeoDataFile(stream, filename), "must throw exception");
        }
    }
}
