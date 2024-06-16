using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Unit tests for WeatherDashboardPage class
    /// </summary>
    [TestClass]
    public class WeatherDashboardPageTest
    {
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
