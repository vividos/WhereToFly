using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Tests for <see cref="CurrentPositionDetailsPage"/> class
    /// </summary>
    [TestClass]
    public class CurrentPositionDetailsPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of page
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // run
            var page = new CurrentPositionDetailsPage(null!);

            // check
            Assert.IsTrue(
                page.Title.Length > 0,
                "page title must have been set");
        }
    }
}
