﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WhereToFly.Geo.DataFormats;

namespace WhereToFly.Geo.UnitTest
{
    /// <summary>
    /// Tests GpxDataFile class
    /// </summary>
    [TestClass]
    public class GpxDataFileTests
    {
        /// <summary>
        /// Tests getting track list, in .gpx format
        /// </summary>
        [TestMethod]
        public void TestGetTrackList()
        {
            // run
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "tracks.gpx");
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var gpxFile = new GpxDataFile(stream);
                var trackList = gpxFile.GetTrackList();

                // check
                Assert.IsTrue(trackList.Count != 0, "track list list must at least one track");
            }
        }

        /// <summary>
        /// Tests loading track, in .gpx format
        /// </summary>
        [TestMethod]
        public void TestLoadTrack()
        {
            // run
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "tracks.gpx");
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var gpxFile = new GpxDataFile(stream);

                var track = gpxFile.LoadTrack(0);

                // check
                Assert.IsTrue(track.TrackPoints.Count != 0, "track points list must not be empty");
            }
        }

        /// <summary>
        /// Tests loading location list, in .gpx format
        /// </summary>
        [TestMethod]
        public void TestLoadLocationList()
        {
            // run
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "waypoints.gpx");
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var gpxFile = new GpxDataFile(stream);

                Assert.IsTrue(gpxFile.HasLocations(), "file must contain locations");
                var locationList = gpxFile.LoadLocationList();

                // check
                Assert.IsTrue(locationList.Count != 0, "loaded location list must contain locations");
            }
        }
    }
}
