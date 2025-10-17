using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.App.Logic;
using WhereToFly.App.Models;
using WhereToFly.App.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Tests LocationListEntryViewModel class
    /// </summary>
    [TestClass]
    public class LocationListEntryViewModelTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests ctor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // set up
            var location = new Location(
                Guid.NewGuid().ToString("B"),
                new MapPoint(47.6764385, 11.8710533, 1685.0))
            {
                Name = "Brecherspitz",
                Description = "Herrliche Aussicht über die drei Seen Schliersee im Norden, Tegernsee im Westen und den Spitzingsee im Süden.",
                Type = LocationType.Summit,
                InternetLink = "https://de.wikipedia.org/wiki/Brecherspitz",
            };

            var parentViewModel = new LocationListViewModel(new AppSettings());
            var viewModel = new LocationListEntryViewModel(parentViewModel, location, null);

            // check
            Assert.IsGreaterThan(0, viewModel.Name.Length, "name text must not be empty");
            Assert.IsGreaterThan(0, viewModel.Description.Length, "description text must not be empty");
        }
    }
}
