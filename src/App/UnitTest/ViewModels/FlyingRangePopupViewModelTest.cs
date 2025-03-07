using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Models;
using WhereToFly.App.Services.SqliteDatabase;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class FlyingRangePopupViewModel
    /// </summary>
    [TestClass]
    public class FlyingRangePopupViewModelTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IDataService, SqliteDatabaseDataService>();
        }

        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestDefaultCtor()
        {
            // set up
            var appSettings = new AppSettings();

            // run
            var viewModel = new FlyingRangePopupViewModel(appSettings);

            // check
            Assert.IsTrue(
                viewModel.GlideRatio > 0.0 &&
                viewModel.GlideRatio < 20.0,
                "glide ratio must have a sane default value");

            Assert.IsTrue(viewModel.WindDirectionList.Count != 0, "wind direction list must contain values");

            Assert.IsTrue(
                viewModel.WindDirectionList.Contains(viewModel.WindDirection),
                "wind direction must be in the wind direction list");

            Assert.IsTrue(viewModel.WindSpeed.Any(), "wind speed must contain text");

            // modify values
            viewModel.GlideRatio = 8.5;
            viewModel.WindDirection = viewModel.WindDirectionList[1];
            viewModel.WindSpeed = "42 km/h";

            Assert.AreEqual(42.0, viewModel.Parameters.WindSpeed, 1e-6, "modified wind speed must match");

            await viewModel.StoreFlyingRangeParameters();
        }
    }
}
