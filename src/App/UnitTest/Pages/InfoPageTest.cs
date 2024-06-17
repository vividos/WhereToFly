using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Tests for <see cref="InfoPage"/> class
    /// </summary>
    [TestClass]
    public class InfoPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of page
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var page = new InfoPage();

            // check
            Assert.IsTrue(page.Title.Length > 0, "page title must have been set");
        }
    }
}
