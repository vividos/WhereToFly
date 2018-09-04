using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Geo;
using WhereToFly.App.Geo.DataFormats;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests class IgcFormatLoader
    /// </summary>
    [TestClass]
    public class IgcFormatLoaderTest
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
        /// Tests method GetTrackList()
        /// </summary>
        [TestMethod]
        public void TestGetTrackList()
        {
            // run
            string filename = Path.Combine(this.TestAssetsPath, "85QA3ET1.igc");
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var trackList = IgcFormatLoader.GetTrackList(stream);

                // check
                Assert.AreEqual(1, trackList.Count, "track list must contain exactly one track");
            }
        }

        /// <summary>
        /// Tests method LoadTrack()
        /// </summary>
        [TestMethod]
        public void TestLoadTrack()
        {
            // run
            string filename = Path.Combine(this.TestAssetsPath, "85QA3ET1.igc");

            Track track = null;
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                track = IgcFormatLoader.LoadTrack(stream, 0);
            }

            // check
            Assert.IsNotNull(track, "track must not be null");
            Assert.IsNotNull(track.Name, "track name must be set");
            Assert.IsTrue(track.TrackPoints.Any(), "there must be any track points");
        }
    }
}
