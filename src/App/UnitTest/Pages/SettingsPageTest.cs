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
            Assert.IsGreaterThan(0, page.Title.Length, "page title must have been set");
            Assert.IsNotEmpty(page.Children, "tabbed page must have at least one sub page");
        }
    }
}
