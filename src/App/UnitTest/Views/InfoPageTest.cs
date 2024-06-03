using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Views;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Tests for InfoPage class
    /// </summary>
    [TestClass]
    public class InfoPageTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IPlatform, UnitTestPlatform>();
        }

        /// <summary>
        /// Tests default ctor of InfoPage
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
