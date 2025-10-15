using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Models;
using WhereToFly.App.Pages;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Tests for <see cref="WeatherDetailsPage"/> class
    /// </summary>
    [TestClass]
    public class WeatherDetailsPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of page
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
            Assert.IsTrue(
                page.BindingContext is WeatherDetailsViewModel viewModel &&
                viewModel.Title.Length > 0,
                "page title must have been set");
        }
    }
}
