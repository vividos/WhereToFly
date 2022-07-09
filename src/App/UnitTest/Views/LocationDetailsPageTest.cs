using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Views;
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
            FFImageLoading.ImageService.EnableMockImageService = true;
            DependencyService.Register<IPlatform, UnitTestPlatform>();
            DependencyService.Register<SvgImageCache>();
            Core.App.Settings = new AppSettings();
        }

        /// <summary>
        /// Tests default ctor of LocationDetailsPage
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var location = UnitTestHelper.GetDefaultLocation();
            var page = new LocationDetailsPage(location);

            // check
            Assert.IsTrue(page.Title.Length > 0, "page title must have been set");
        }
    }
}
