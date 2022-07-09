using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.ViewModels;
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
                WebLink = "com.windyty.android",
            };

            // run
            var viewModel = new WeatherIconViewModel(null, description);

            ////Assert.IsTrue(
            ////    viewModel.WaitForPropertyChange(
            ////        nameof(viewModel.Icon),
            ////        TimeSpan.FromSeconds(10)),
            ////    "waiting for property change must succeed");

            // check
            Assert.AreEqual(description.Name, viewModel.Title, "title must match name of description");
            ////Assert.IsNotNull(viewModel.Icon, "icon image source must not be null");
            Assert.IsNotNull(viewModel.Tapped, "Tapped command must not be null");
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
                WebLink = "com.windyty.android",
            };

            // run
            var viewModel = new WeatherIconViewModel(null, description);
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
                WebLink = "https://localhost/test/123/",
            };

            // run
            var viewModel = new WeatherIconViewModel(null, description);
            Assert.IsTrue(viewModel.Tapped.CanExecute(null), "command must be able to executed");

            viewModel.Tapped.Execute(null);

            // can't check result
        }
    }
}
