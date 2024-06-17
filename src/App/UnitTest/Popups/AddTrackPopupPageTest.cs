using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Popups;

namespace WhereToFly.App.UnitTest.Popups
{
    /// <summary>
    /// Tests for AddTrackPopupPage class
    /// </summary>
    [TestClass]
    public class AddTrackPopupPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of popup page
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var track = UnitTestHelper.GetDefaultTrack();
            var page = new AddTrackPopupPage(track);

            // check
            Assert.IsNotNull(page.Content, "page content must have been set");
        }
    }
}
