using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Views;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Tests for LocationDetailsPage class
    /// </summary>
    [TestClass]
    public class LocationDetailsPageTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
            DependencyService.Register<SvgImageCache>();
            Core.App.Settings = new AppSettings();
        }

        /// <summary>
        /// Returns default location for tests
        /// </summary>
        /// <returns>default location</returns>
        private static Location GetDefaultLocation()
        {
            return new Location
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = "Brecherspitz",
                MapLocation = new MapPoint(47.6764385, 11.8710533, 1685.0),
                Description = "Herrliche Aussicht über die drei Seen Schliersee im Norden, Tegernsee im Westen und den Spitzingsee im Süden.",
                Type = LocationType.Summit,
                InternetLink = "https://de.wikipedia.org/wiki/Brecherspitz"
            };
        }

        /// <summary>
        /// Tests default ctor of LocationDetailsPage
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var location = GetDefaultLocation();
            var page = new LocationDetailsPage(location);

            // check
            Assert.IsTrue(page.Title.Length > 0, "page title must have been set");
        }
    }
}
