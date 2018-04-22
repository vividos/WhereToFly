using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for LocationListViewModel
    /// </summary>
    [TestClass]
    public class LocationListViewModelTest
    {
        /// <summary>
        /// Called before each test method; initializes dependency service with DataService
        /// instance.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IDataService, DataService>();
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

            var propertyChangedEvent = new ManualResetEvent(false);

            viewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(viewModel.LocationList))
                    {
                        propertyChangedEvent.Set();
                    }
                };

            propertyChangedEvent.WaitOne();

            // check
            Assert.IsFalse(viewModel.LocationList.Any(), "location list is initially empty");
            Assert.AreEqual(0, viewModel.FilterText.Length, "filter text is initially empty");
            Assert.IsFalse(viewModel.AreAllLocationsFilteredOut, "as there are no locations, no location was filtered out");            
        }
    }
}
