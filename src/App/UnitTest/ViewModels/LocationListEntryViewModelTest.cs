using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Logic.Model;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Tests LocationListEntryViewModel class
    /// </summary>
    [TestClass]
    public class LocationListEntryViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
        }

        /// <summary>
        /// Tests ctor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // set up
            var location = new Location
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = "Brecherspitz",
                Elevation = 1685,
                MapLocation = new MapPoint(47.6764385, 11.8710533),
                Description = "Herrliche Aussicht über die drei Seen Schliersee im Norden, Tegernsee im Westen und den Spitzingsee im Süden.",
                Type = LocationType.Summit,
                InternetLink = "https://de.wikipedia.org/wiki/Brecherspitz"
            };

            var viewModel = new LocationListEntryViewModel(null, location, null);

            // check
            Assert.IsTrue(viewModel.Name.Length > 0, "name text must not be empty");
            Assert.IsTrue(viewModel.Description.Length > 0, "description text must not be empty");
        }
    }
}
