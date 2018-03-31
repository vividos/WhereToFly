using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WhereToFly.Core.ViewModels;
using WhereToFly.Logic.Model;

namespace WhereToFly.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for LocationListViewModel
    /// </summary>
    [TestClass]
    public class LocationListViewModelTest
    {
        /// <summary>
        /// Tests ctor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // set up
            var appSettings = new AppSettings();
            var viewModel = new LocationListViewModel(appSettings);

            // check
            Assert.IsFalse(viewModel.LocationList.Any(), "location list is initially empty");
            Assert.AreEqual(0, viewModel.FilterText.Length, "filter text is initially empty");
            Assert.IsFalse(viewModel.AreAllLocationsFilteredOut, "as there are no locations, no location was filtered out");            
        }
    }
}
