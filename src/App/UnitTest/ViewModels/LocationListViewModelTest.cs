using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using WhereToFly.App.Models;
using WhereToFly.App.Services.SqliteDatabase;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for LocationListViewModel
    /// </summary>
    [TestClass]
    public class LocationListViewModelTest
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
        /// Tests ctor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // set up
            var appSettings = new AppSettings();
            var viewModel = new LocationListViewModel(appSettings);

            Assert.IsTrue(
                viewModel.WaitForPropertyChange(
                    nameof(viewModel.LocationList),
                    TimeSpan.FromSeconds(10)),
                "waiting for property change must succeed");

            // check
            Assert.IsNotNull(viewModel.LocationList, "location list must be available");
            Assert.IsTrue(viewModel.LocationList.Any(), "location list initially contains the default locations");
            Assert.AreEqual(0, viewModel.FilterText.Length, "filter text is initially empty");
            Assert.IsFalse(viewModel.AreAllLocationsFilteredOut, "as there is no filter text, no location was filtered out");
        }
    }
}
