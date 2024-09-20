using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class WeatherDashboardViewModel
    /// </summary>
    [TestClass]
    public class WeatherDashboardViewModelTest : UserInterfaceTestBase
    {
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
