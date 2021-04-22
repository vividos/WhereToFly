using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using WhereToFly.Geo;
using WhereToFly.Geo.DataFormats;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests for track extension methods
    /// </summary>
    [TestClass]
    public class TrackExtensionMethodsTest
    {
        /// <summary>
        /// Tests calculating statistics on an empty track
        /// </summary>
        [TestMethod]
        public void TestCalculateStatistics_EmptyTrack()
        {
            // set up
            var track = new Track();

            // run
            track.CalculateStatistics();

            // check
            Assert.AreEqual(TimeSpan.Zero, track.Duration, "duration must be zero");
        }

        /// <summary>
        /// Tests CalculateStatistics() on all tracks that can be loaded from assets.
        /// </summary>
        [TestMethod]
        public void TestCalculateStatistics_AllAssetTracks()
        {
            // set up
            string[] trackFilenames =
            {
                "waypoints.kml",
                "track_linestring.kmz",
                "tracks.gpx",
                "tracks.kmz",
                "85QA3ET1.igc",
            };

            // run
            foreach (string trackFilename in trackFilenames)
            {
                string filename = Path.Combine(UnitTestHelper.TestAssetsPath, trackFilename);
                var geoDataFile = GeoLoader.LoadGeoDataFile(filename);

                var trackList = geoDataFile.GetTrackList();
                for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
                {
                    var track = geoDataFile.LoadTrack(trackIndex);
                    if (track != null)
                    {
                        track.CalculateStatistics();

                        // check
                        Assert.IsNotNull(track.Id, "track ID must not be null");
                    }
                }
            }
        }
    }
}
