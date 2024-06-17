using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Tests for <see cref="EditLocationDetailsPage"/> class
    /// </summary>
    [TestClass]
    public class EditLocationDetailsPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of page
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestDefaultCtor()
        {
            // set up
            var location = UnitTestHelper.GetDefaultLocation();

            // run
            var root = new ContentPage();
            var page = new EditLocationDetailsPage(location);

            await root.Navigation.PushAsync(page);

            // check
            Assert.IsTrue(page.Title.Length > 0, "page title must have been set");
        }
    }
}
