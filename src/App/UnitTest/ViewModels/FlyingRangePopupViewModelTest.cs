using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Services.SqliteDatabase;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class FlyingRangePopupViewModel
    /// </summary>
    [TestClass]
    public class FlyingRangePopupViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks and DependencyService with
        /// DataService and Platform.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IDataService, SqliteDatabaseDataService>();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
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

            Assert.IsTrue(viewModel.WindDirectionList.Any(), "wind direction list must contain values");

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
