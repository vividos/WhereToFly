using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using WhereToFly.App.Services.SqliteDatabase;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class WeatherDashboardViewModel
    /// </summary>
    [TestClass]
    public class WeatherDashboardViewModelTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IDataService, SqliteDatabaseDataService>();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
        }

        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // run
            var viewModel = new WeatherDashboardViewModel();

            Assert.IsTrue(
                viewModel.WaitForPropertyChange(
                    nameof(viewModel.WeatherDashboardItems),
                    TimeSpan.FromSeconds(10)),
                "waiting for property change must succeed");

            // check
            Assert.IsTrue(viewModel.WeatherDashboardItems.Any(), "weather icon list must contain placeholder icon");
        }
    }
}
