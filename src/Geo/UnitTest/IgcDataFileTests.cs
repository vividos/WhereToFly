using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using WhereToFly.Geo.DataFormats;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo.UnitTest
{
    /// <summary>
    /// Tests class IgcDataFile
    /// </summary>
    [TestClass]
    public class IgcDataFileTests
    {
        /// <summary>
        /// Tests method GetTrackList()
        /// </summary>
        [TestMethod]
        public void TestGetTrackList()
        {
            // run
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "85QA3ET1.igc");
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var igcFile = new IgcDataFile(stream);
                var trackList = igcFile.GetTrackList();

                // check
                Assert.HasCount(1, trackList, "track list must contain exactly one track");
            }
        }

        /// <summary>
        /// Tests method LoadTrack()
        /// </summary>
        [TestMethod]
        public void TestLoadTrack()
        {
            // run
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "85QA3ET1.igc");

            Track? track = null;
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var igcFile = new IgcDataFile(stream);
                track = igcFile.LoadTrack(0);
            }

            // check
            Assert.IsNotNull(track, "track must not be null");
            Assert.IsNotNull(track.Name, "track name must be set");
            Assert.IsNotEmpty(track.TrackPoints, "there must be any track points");
        }

        /// <summary>
        /// Tests not implemented methods HasLocations() and LoadLocationList()
        /// </summary>
        [TestMethod]
        public void TestNotImplementedLocationsMethods()
        {
            // run
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "85QA3ET1.igc");

            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var igcFile = new IgcDataFile(stream);

                // check
                Assert.IsFalse(igcFile.HasLocations(), "IGC file must not contain locations");
                Assert.ThrowsExactly<NotImplementedException>(
                    igcFile.LoadLocationList,
                    "loading locations from igc file must throw exception");
            }
        }
    }
}
