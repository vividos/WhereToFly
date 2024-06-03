using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Logic;
using WhereToFly.App.Services;
using WhereToFly.App.Views;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Tests for LayerDetailsPage class
    /// </summary>
    [TestClass]
    public class LayerDetailsPageTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IPlatform, UnitTestPlatform>();
            DependencyService.Register<SvgImageCache>();
        }

        /// <summary>
        /// Tests default ctor of LayerDetailsPage
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestDefaultCtor()
        {
            // set up
            var layerList = await DataServiceHelper.GetInitialLayerList();
            var page = new LayerDetailsPage(layerList.First());

            // check
            Assert.IsTrue(page.Title.Length > 0, "page title must have been set");
            Assert.IsNotNull(page.Content, "page content must have been set");
        }
    }
}
