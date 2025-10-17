using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.ViewModels;
using WhereToFly.Geo;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class AddTrackPopupViewModel
    /// </summary>
    [TestClass]
    public class AddTrackPopupViewModelTest
    {
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
            Assert.IsNotNull(viewModel.SelectedTrackColor, "selected track color must be set");

            // modify values
            viewModel.TrackName = "Track2";
            viewModel.SelectedTrackColor = Color.FromArgb("#0000FF");
            viewModel.IsFlightTrack = false;
        }
    }
}
