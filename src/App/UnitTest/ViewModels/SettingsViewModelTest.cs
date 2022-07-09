using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class SettingsViewModel
    /// </summary>
    [TestClass]
    public class SettingsViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing app settings object
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Core.App.Settings = new AppSettings();
        }

        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // run
            var viewModel = new MapSettingsViewModel();

            Debug.WriteLine($"SelectedMapImageryType = {viewModel.SelectedMapImageryType}");
            Debug.WriteLine($"SelectedMapOverlayType = {viewModel.SelectedMapOverlayType}");
            Debug.WriteLine($"SelectedMapShadingModeItem = {viewModel.SelectedMapShadingModeItem}");
            Debug.WriteLine($"SelectedCoordinateDisplayFormatItem = {viewModel.SelectedCoordinateDisplayFormatItem}");

            viewModel.SelectedMapImageryType = viewModel.MapImageryTypeItems.Last();
            viewModel.SelectedMapOverlayType = viewModel.MapOverlayTypeItems.Last();
            viewModel.SelectedMapShadingModeItem = viewModel.MapShadingModeItems.Last();
            viewModel.SelectedCoordinateDisplayFormatItem = viewModel.CoordinateDisplayFormatItems.Last();

            // check
            Assert.IsTrue(viewModel.MapImageryTypeItems.Any(), "map imagery type list must contain items");
            Assert.IsTrue(viewModel.MapOverlayTypeItems.Any(), "map overlay type list must contain items");
            Assert.IsTrue(viewModel.MapShadingModeItems.Any(), "map shading mode list must contain items");
            Assert.IsTrue(viewModel.CoordinateDisplayFormatItems.Any(), "coordinate display format list must contain items");
        }
    }
}
