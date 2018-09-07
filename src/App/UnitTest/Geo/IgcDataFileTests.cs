using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using WhereToFly.App.Geo;
using WhereToFly.App.Geo.DataFormats;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests class IgcDataFile
    /// </summary>
    [TestClass]
    public class IgcDataFileTests
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
                var igcFile = new IgcDataFile(stream);
                var trackList = igcFile.GetTrackList();

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
                var igcFile = new IgcDataFile(stream);
                track = igcFile.LoadTrack(0);
            }

            // check
            Assert.IsNotNull(track, "track must not be null");
            Assert.IsNotNull(track.Name, "track name must be set");
            Assert.IsTrue(track.TrackPoints.Any(), "there must be any track points");
        }
    }
}
