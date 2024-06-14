using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Popups;

namespace WhereToFly.App.UnitTest.Popups
{
    /// <summary>
    /// Tests for AddTrackPopupPage class
    /// </summary>
    [TestClass]
    public class AddTrackPopupPageTest
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
        /// Tests default ctor of AddTrackPopupPage
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
