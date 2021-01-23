using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.Views;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Tests for LayerDetailsPage class
    /// </summary>
    [TestClass]
    public class LayerDetailsPageTest
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
        }

        /// <summary>
        /// Tests default ctor of LayerDetailsPage
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var layerList = DataServiceHelper.GetInitialLayerList();
            var page = new LayerDetailsPage(layerList.First());

            // check
            Assert.IsTrue(page.Title.Length > 0, "page title must have been set");
            Assert.IsNotNull(page.Content, "page content must have been set");
        }
    }
}
