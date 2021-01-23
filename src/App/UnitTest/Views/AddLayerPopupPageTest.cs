using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.Views;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Tests for AddLayerPopupPage class
    /// </summary>
    [TestClass]
    public class AddLayerPopupPageTest
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
        }

        /// <summary>
        /// Tests default ctor of AddLayerPopupPage
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var layerList = DataServiceHelper.GetInitialLayerList();
            var page = new AddLayerPopupPage(layerList.First());

            // check
            Assert.IsNotNull(page.Content, "page content must have been set");
        }
    }
}
