using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.WebApi.Logic.Services;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

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
            Assert.IsNotEmpty(
                data.TrackPoints,
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
            Assert.IsNotEmpty(
                track.TrackPoints,
                "there must be any track points");

            ////GpxWriter.WriteTrack("...\\testlivetrack.gpx", track);
        }
    }
}
