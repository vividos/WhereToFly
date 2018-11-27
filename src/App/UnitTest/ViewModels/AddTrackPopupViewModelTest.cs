using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WhereToFly.App.Core;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Geo.Spatial;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class AddTrackPopupViewModel
    /// </summary>
    [TestClass]
    public class AddTrackPopupViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks and DependencyService with
        /// DataService.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
        }

        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var track = UnitTestHelper.GetDefaultTrack();
            track.CalculateStatistics();

            // run
            var viewModel = new AddTrackPopupViewModel(track);

            // check
            Assert.AreEqual(track.Name, viewModel.TrackName, "is not a flight track");
            Assert.AreEqual(track.IsFlightTrack, viewModel.IsFlightTrack, "is not a flight track");
            Assert.IsTrue(viewModel.IsColorPickerVisible, "color picker must be visible");
            Assert.IsTrue(viewModel.SelectedTrackColor.Any(), "selected track color must contain value");

            // modify values
            viewModel.TrackName = "Track2";
            viewModel.SelectedTrackColor = "0000FF";
            viewModel.IsFlightTrack = false;
        }
    }
}
