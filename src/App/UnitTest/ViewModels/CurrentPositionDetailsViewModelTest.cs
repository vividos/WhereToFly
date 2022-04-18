using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Model;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Tests CurrentPositionDetailsViewModel class
    /// </summary>
    [TestClass]
    public class CurrentPositionDetailsViewModelTest
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
            var appSettings = new AppSettings();
            var viewModel = new CurrentPositionDetailsViewModel(appSettings);

            // check
            Assert.IsFalse(viewModel.IsHeadingAvail, "initially heading is not available");
        }

        /// <summary>
        /// Tests updating location when altitude value is unset
        /// </summary>
        [TestMethod]
        public void TestUnsetAltitude()
        {
            // set up
            var appSettings = new AppSettings();
            var viewModel = new CurrentPositionDetailsViewModel(appSettings);

            // run
            var location = new Xamarin.Essentials.Location(48.1, 11.8);
            viewModel.OnPositionChanged(
                this,
                new Core.GeolocationEventArgs(location));

            // check
            Assert.AreEqual(string.Empty, viewModel.Altitude, "accessing altitude must not crash");
            Assert.AreEqual(string.Empty, viewModel.Accuracy, "accessing accuracy must not crash");
            Assert.AreEqual(0, viewModel.HeadingInDegrees, "accessing heading must not crash");
        }
    }
}
