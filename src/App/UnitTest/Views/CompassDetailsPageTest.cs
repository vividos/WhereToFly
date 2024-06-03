using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Models;
using WhereToFly.App.Views;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Tests for CompassDetailsPage class
    /// </summary>
    [TestClass]
    public class CompassDetailsPageTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IPlatform, UnitTestPlatform>();
            DependencyService.Register<IGeolocationService, UnitTestGeolocationService>();
            App.Settings = new AppSettings();
        }

        /// <summary>
        /// Tests default ctor of CompassDetailsPage
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // run
            var page = new CompassDetailsPage();

            // check
            Assert.IsTrue(page.Title.Length > 0, "page title must have been set");
        }
    }
}
