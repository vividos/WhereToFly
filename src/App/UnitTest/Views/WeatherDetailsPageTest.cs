using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Views;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Tests for WeatherDetailsPage class
    /// </summary>
    [TestClass]
    public class WeatherDetailsPageTest
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
        /// Tests default ctor of WeatherDetailsPage
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var iconDescription = new WeatherIconDescription
            {
                Name = "TestLink",
                Type = WeatherIconDescription.IconType.IconLink,
                WebLink = "https://localhost/test/123/",
            };

            // run
            var page = new WeatherDetailsPage(iconDescription);

            // check
            Assert.IsTrue(page.Title.Length > 0, "page title must have been set");
        }
    }
}
