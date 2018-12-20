using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.Shared.Model;
using WhereToFly.WebApi.Logic.Services;

namespace WhereToFly.WebApi.UnitTest
{
    /// <summary>
    /// Tests for the Find Me SPOT web service class
    /// </summary>
    [TestClass]
    public class FindMeSpotTest
    {
        /// <summary>
        /// Test to get live waypoint data using data service
        /// </summary>
        [TestMethod]
        public void TestLiveWaypointData()
        {
            // set up
            var dataService = new FindMeSpotTrackerDataService();
            string uri = new AppResourceUri(AppResourceUri.ResourceType.FindMeSpotPos, "xxx").ToString();

            // run
            var liveWaypointData = dataService.GetDataAsync(uri).Result;

            // check
            Assert.AreEqual(uri, liveWaypointData.ID, "requested ID and app resource URI");
        }
    }
}
