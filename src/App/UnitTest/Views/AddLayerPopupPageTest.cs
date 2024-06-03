using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Services;
using WhereToFly.App.Views;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Tests for AddLayerPopupPage class
    /// </summary>
    [TestClass]
    public class AddLayerPopupPageTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IPlatform, UnitTestPlatform>();
        }

        /// <summary>
        /// Tests default ctor of AddLayerPopupPage
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestDefaultCtor()
        {
            // set up
            var layerList = await DataServiceHelper.GetInitialLayerList();
            var page = new AddLayerPopupPage(layerList.First());

            // check
            Assert.IsNotNull(page.Content, "page content must have been set");
        }
    }
}
