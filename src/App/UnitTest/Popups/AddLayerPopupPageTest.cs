using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Popups;
using WhereToFly.App.Services;

namespace WhereToFly.App.UnitTest.Popups
{
    /// <summary>
    /// Tests for <see cref="AddLayerPopupPage"/> class
    /// </summary>
    [TestClass]
    public class AddLayerPopupPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of popup page
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
