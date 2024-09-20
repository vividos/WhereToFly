using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Models;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for SelectWeatherIconViewModel
    /// </summary>
    [TestClass]
    public class SelectWeatherIconViewModelTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests ctor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // set up + run
            var tcs = new TaskCompletionSource<WeatherIconDescription>();
            var viewModel = new SelectWeatherIconViewModel(
                (result) => tcs.SetResult(result));

            Assert.IsTrue(
                viewModel.WaitForPropertyChange(
                    nameof(viewModel.GroupedWeatherIconList),
                    TimeSpan.FromSeconds(10)),
                "waiting for property change must succeed");

            // check
            Assert.IsNotNull(
                viewModel.GroupedWeatherIconList,
                "grouped weather icon list must be available");

            Assert.IsTrue(
                viewModel.GroupedWeatherIconList.Any(),
                "weather icon description list must not be empty");
        }
    }
}
