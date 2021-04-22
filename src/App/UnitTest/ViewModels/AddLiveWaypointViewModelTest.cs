using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class AddLiveWaypointPopupViewModel
    /// </summary>
    [TestClass]
    public class AddLiveWaypointPopupViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
        }

        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var location = UnitTestHelper.GetDefaultLocation();
            location.Type = LocationType.LiveWaypoint;

            // run
            var viewModel = new AddLiveWaypointPopupViewModel(location);

            // check
            Assert.AreEqual(location.Name, viewModel.Name, "location mame must match");
            Assert.IsTrue(viewModel.Type.Any(), "location type must contain text");

            // modify values
            viewModel.Name = "Live42";
        }
    }
}
