using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Views;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Tests for CompassDetailsPage class
    /// </summary>
    [TestClass]
    public class CompassDetailsPageTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
            DependencyService.Register<IGeolocationService, UnitTestGeolocationService>();
            Core.App.Settings = new AppSettings();
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
