using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core.Views;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Tests for AddLiveWaypointPopupPage class
    /// </summary>
    [TestClass]
    public class AddLiveWaypointPopupPageTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            FFImageLoading.ImageService.EnableMockImageService = true;
        }

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
