using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Views;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Unit tests for SettingsPage
    /// </summary>
    [TestClass]
    public class SettingsPageTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
            Core.App.Settings = new AppSettings();
        }

        /// <summary>
        /// Tests default ctor of SettingsPage
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var page = new SettingsPage();

            // check
            Assert.IsTrue(page.Title.Length > 0, "page title must have been set");
            Assert.IsTrue(page.Children.Count > 0, "tabbed page must have at least one sub page");
        }
    }
}
