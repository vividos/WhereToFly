using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Models;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Tests for WeatherDetailsPage class
    /// </summary>
    [TestClass]
    public class WeatherDetailsPageTest
    {
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
