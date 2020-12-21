using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core.Views;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Unit tests for WeatherDashboardPage class
    /// </summary>
    [TestClass]
    public class WeatherDashboardPageTest
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
        /// Tests default ctor of WeatherDashboardPage
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var page = new WeatherDashboardPage();

            // check
            Assert.IsTrue(page.Title.Length > 0, "page title must have been set");
        }
    }
}
