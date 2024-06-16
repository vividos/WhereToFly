using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Models;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Unit tests for SettingsPage
    /// </summary>
    [TestClass]
    public class SettingsPageTest
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
