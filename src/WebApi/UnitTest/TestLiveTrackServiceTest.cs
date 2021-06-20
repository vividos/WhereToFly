using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using WhereToFly.WebApi.Logic.Services;

namespace WhereToFly.WebApi.UnitTest
{
    /// <summary>
    /// Tests for TestLiveTrackService class
    /// </summary>
    [TestClass]
    public class TestLiveTrackServiceTest
    {
        /// <summary>
        /// Test for getting live tracking data
        /// </summary>
        [TestMethod]
        public void TestGetLiveTrackingData()
        {
            // run
            var data = TestLiveTrackService.GetLiveTrackingData("id", DateTimeOffset.Now);

            // check
            Assert.IsTrue(
                data.TrackPoints.Any(),
                "there must be any track points");
        }

        /// <summary>
        /// Tests generating live track data
        /// </summary>
        [TestMethod]
        public void TestGenerateLiveTrackData()
        {
            // run
            var track = TestLiveTrackService.GenerateLiveTrackData(DateTimeOffset.Now);

            // check
            Assert.IsTrue(
                track.TrackPoints.Any(),
                "there must be any track points");

            ////GpxWriter.WriteTrack("...\\testlivetrack.gpx", track);
        }
    }
}
