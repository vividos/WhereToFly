using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Unit tests for <see cref="SettingsPage"/>
    /// </summary>
    [TestClass]
    public class SettingsPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of page
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
