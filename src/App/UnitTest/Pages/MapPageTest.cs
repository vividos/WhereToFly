using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Abstractions;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Tests for MapPage class
    /// </summary>
    [TestClass]
    public class MapPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of page
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var page = new MapPage(this.Services);

            // check
            Assert.IsGreaterThan(0, page.Title.Length, "page title must have been set");
        }
    }
}
