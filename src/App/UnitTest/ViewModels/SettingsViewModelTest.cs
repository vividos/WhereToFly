using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class SettingsViewModel
    /// </summary>
    [TestClass]
    public class SettingsViewModelTest : UserInterfaceTestBase
    {
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
            Assert.IsNotEmpty(viewModel.MapImageryTypeItems, "map imagery type list must contain items");
            Assert.IsNotEmpty(viewModel.MapOverlayTypeItems, "map overlay type list must contain items");
            Assert.IsNotEmpty(viewModel.MapShadingModeItems, "map shading mode list must contain items");
            Assert.IsNotEmpty(viewModel.CoordinateDisplayFormatItems, "coordinate display format list must contain items");
        }
    }
}
