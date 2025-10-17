using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Logic;
using WhereToFly.App.Pages;
using WhereToFly.App.Services;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Tests for <see cref="LayerDetailsPage"/> class
    /// </summary>
    [TestClass]
    public class LayerDetailsPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of page
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestDefaultCtor()
        {
            // set up
            var layerList = await DataServiceHelper.GetInitialLayerList();
            var page = new LayerDetailsPage(layerList.First());

            // check
            Assert.IsGreaterThan(0, page.Title.Length, "page title must have been set");
            Assert.IsNotNull(page.Content, "page content must have been set");
        }
    }
}
