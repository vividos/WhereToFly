using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for SelectWeatherIconViewModel
    /// </summary>
    [TestClass]
    public class SelectWeatherIconViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks and DependencyService with
        /// DataService.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IDataService, DataService>();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
        }

        /// <summary>
        /// Tests ctor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // set up + run
            var viewModel = new SelectWeatherIconViewModel((iconDescription) => { });

            Assert.IsTrue(
                viewModel.WaitForPropertyChange(
                    nameof(viewModel.WeatherIconList),
                    TimeSpan.FromSeconds(10)),
                "waiting for property change must succeed");

            // check
            Assert.IsTrue(viewModel.WeatherIconList.Any(), "weather icon description list must not be empty");
        }
    }
}
