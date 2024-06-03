using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using WhereToFly.App.Models;
using WhereToFly.App.Views;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Tests for EditLocationDetailsPage class
    /// </summary>
    [TestClass]
    public class EditLocationDetailsPageTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IPlatform, UnitTestPlatform>();
            App.Settings = new AppSettings();
        }

        /// <summary>
        /// Tests default ctor of EditLocationDetailsPage
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
