using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo;
using Xamarin.Forms;

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
            Assert.AreEqual(string.Empty, viewModel.Longitude, "longitude text must be correct");
            Assert.AreEqual(string.Empty, viewModel.Latitude, "latitude text must be correct");
            Assert.AreEqual(string.Empty, viewModel.Altitude, "altitude text must be correct");
            Assert.AreEqual(string.Empty, viewModel.Accuracy, "accuracy text must be correct");
            Assert.AreEqual(Color.Black, viewModel.PositionAccuracyColor, "accuracy color must be black");
            Assert.AreEqual("Unknown", viewModel.LastPositionFix, "last position fix text must be correct");
            Assert.AreEqual(0, viewModel.SpeedInKmh, "speed value must be correct");
            Assert.IsFalse(viewModel.IsMagneticNorthHeadingAvail, "initially heading is not available");
            Assert.AreEqual(0, viewModel.MagneticNorthHeadingInDegrees, "heading value must be correct");
            Assert.IsFalse(viewModel.IsSunriseSunsetAvail, "sunrise/sunset must not be available");
            Assert.AreEqual("N/A", viewModel.SunriseTime, "sunrise time text must be correct");
            Assert.AreEqual("N/A", viewModel.SunsetTime, "sunset time text must be correct");
        }

        /// <summary>
        /// Tests updating the location values
        /// </summary>
        [TestMethod]
        public void TestLocationUpdate()
        {
            // set up
            var appSettings = new AppSettings
            {
                CoordinateDisplayFormat = CoordinateDisplayFormat.Format_dd_mm_sss,
            };

            var viewModel = new CurrentPositionDetailsViewModel(appSettings);

            // run
            var location = new Xamarin.Essentials.Location(48.137222, 11.575556, 512)
            {
                AltitudeReferenceSystem = Xamarin.Essentials.AltitudeReferenceSystem.Geoid,
                Accuracy = 42,
                Course = 64.2,
                Speed = 4.1,
                Timestamp = new DateTimeOffset(2022, 4, 19, 7, 44, 0, TimeSpan.FromHours(2)),
                IsFromMockProvider = true,
            };

            viewModel.OnPositionChanged(
                this,
                new Core.GeolocationEventArgs(location));

            // check
            Assert.AreEqual("11° 34' 32\"", viewModel.Longitude, "longitude text must be correct");
            Assert.AreEqual("48° 8' 13\"", viewModel.Latitude, "latitude text must be correct");
            Assert.AreEqual("512", viewModel.Altitude, "altitude text must be correct");
            Assert.AreEqual("42", viewModel.Accuracy, "accuracy text must be correct");
            Assert.AreEqual(Color.FromHex("#E0E000"), viewModel.PositionAccuracyColor, "accuracy color must be black");
            Assert.IsTrue(viewModel.LastPositionFix.Length > 0, "last position fix text must contain text");
            Assert.AreEqual(14, viewModel.SpeedInKmh, "speed value must be correct");
            Assert.IsFalse(viewModel.IsMagneticNorthHeadingAvail, "initially magnetic-north heading is not available");
            Assert.AreEqual(0, viewModel.MagneticNorthHeadingInDegrees, "magnetic-north heading value must be correct");
            Assert.IsTrue(viewModel.IsTrueNorthHeadingAvail, "true-north heading is available");
            Assert.AreEqual(64, viewModel.TrueNorthHeadingInDegrees, "true-north heading value must be correct");
            Assert.IsTrue(viewModel.IsSunriseSunsetAvail, "sunrise/sunset must not be available");
            Assert.AreEqual("6:13:05", viewModel.SunriseTime, "sunrise time text must be correct");
            Assert.AreEqual("20:14:49", viewModel.SunsetTime, "sunset time text must be correct");
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
            var location = new Xamarin.Essentials.Location(48.137222, 11.575556);
            viewModel.OnPositionChanged(
                this,
                new Core.GeolocationEventArgs(location));

            // check
            Assert.AreEqual(string.Empty, viewModel.Altitude, "accessing altitude must not crash");
            Assert.AreEqual(string.Empty, viewModel.Accuracy, "accessing accuracy must not crash");
            Assert.AreEqual(0, viewModel.MagneticNorthHeadingInDegrees, "accessing heading must not crash");
        }
    }
}
