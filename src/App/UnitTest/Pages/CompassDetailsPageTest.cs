using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Tests for <see cref="CompassDetailsPage"/> class
    /// </summary>
    [TestClass]
    public class CompassDetailsPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of page
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // run
            var page = new CompassDetailsPage(null!);

            // check
            Assert.IsGreaterThan(
                0,
                page.Title.Length,
                "page title must have been set");
        }
    }
}
