using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Tests for <see cref="LocationDetailsPage"/> class
    /// </summary>
    [TestClass]
    public class LocationDetailsPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of page
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var location = UnitTestHelper.GetDefaultLocation();
            var page = new LocationDetailsPage(location);

            // check
            Assert.IsGreaterThan(0, page.Title.Length, "page title must have been set");
        }
    }
}
