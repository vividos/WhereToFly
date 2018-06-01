using System;
using System.ComponentModel;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class WeatherIconViewModel
    /// </summary>
    [TestClass]
    public class WeatherIconViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks and app manager instance
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IAppManager, UnitTestAppManager>();
            DependencyService.Register<WeatherImageCache>();
        }

        /// <summary>
        /// Tests view model default ctor
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var description = new WeatherIconDescription
            {
                Name = "Windy",
                Type = WeatherIconDescription.IconType.IconApp,
                WebLink = "com.windyty.android"
            };

            // run
            var viewModel = new WeatherIconViewModel(description);

            ////Assert.IsTrue(WaitForPropertyChange(viewModel, nameof(viewModel.Icon)));

            // check
            Assert.AreEqual(description.Name, viewModel.Title, "title must match name of description");
            ////Assert.IsNotNull(viewModel.Icon, "icon image source must not be null");
            Assert.IsNotNull(viewModel.Tapped, "Tapped command must not be null");
        }

        /// <summary>
        /// Waits for a PropertyChanged event from view model implementing INotifyPropertyChanged.
        /// Waits 10 seconds before returning.
        /// </summary>
        /// <param name="viewModel">view model to wait on</param>
        /// <param name="propertyName">name of property that has to change</param>
        /// <returns>true when property was changed, or false when not</returns>
        private static bool WaitForPropertyChange(INotifyPropertyChanged viewModel, string propertyName)
        {
            var propertyChangedEvent = new ManualResetEvent(false);

            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                {
                    propertyChangedEvent.Set();
                }
            };

            return propertyChangedEvent.WaitOne(10 * 1000); // wait maximum number of seconds
        }

        /// <summary>
        /// Tests tapping an IconApp weather icon
        /// </summary>
        [TestMethod]
        public void TestTappedIconTypeIconApp()
        {
            // set up
            var description = new WeatherIconDescription
            {
                Name = "Windy",
                Type = WeatherIconDescription.IconType.IconApp,
                WebLink = "com.windyty.android"
            };

            // run
            var viewModel = new WeatherIconViewModel(description);
            Assert.IsTrue(viewModel.Tapped.CanExecute(null), "command must be able to executed");

            viewModel.Tapped.Execute(null);

            // check
            var appManager = DependencyService.Get<IAppManager>() as UnitTestAppManager;
            Assert.IsTrue(appManager.AppHasBeenOpened, "app must have been opened");
        }

        /// <summary>
        /// Tests tapping an IconLink weather icon
        /// </summary>
        [TestMethod]
        public void TestTappedIconTypeIconLink()
        {
            // set up
            var description = new WeatherIconDescription
            {
                Name = "Windy",
                Type = WeatherIconDescription.IconType.IconLink,
                WebLink = "https://localhost/test/123/"
            };

            // run
            var viewModel = new WeatherIconViewModel(description);
            Assert.IsTrue(viewModel.Tapped.CanExecute(null), "command must be able to executed");

            viewModel.Tapped.Execute(null);

            // can't check result
        }
    }
}
