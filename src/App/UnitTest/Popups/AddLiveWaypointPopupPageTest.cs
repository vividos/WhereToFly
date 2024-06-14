using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Popups;
using WhereToFly.App.Views;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest.Popups
{
    /// <summary>
    /// Tests for AddLiveWaypointPopupPage class
    /// </summary>
    [TestClass]
    public class AddLiveWaypointPopupPageTest
    {
        /// <summary>
        /// Tests default ctor of AddLiveWaypointPopupPage
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var location = new Location(
                "test-id",
                new MapPoint(47.6764385, 11.8710533, 1685.0))
            {
                Name = "Test Live Waypoint",
                Type = LocationType.LiveWaypoint,
                InternetLink = "where-to-fly://xxx",
            };
            var page = new AddLiveWaypointPopupPage(location);

            // check
            Assert.IsNotNull(page.Content, "page content must have been set");
        }
    }
}
