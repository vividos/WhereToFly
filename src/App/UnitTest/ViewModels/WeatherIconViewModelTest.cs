using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using WhereToFly.App.Models;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class WeatherIconViewModel
    /// </summary>
    [TestClass]
    public class WeatherIconViewModelTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IUserInterface, UnitTestUserInterface>();
            DependencyService.Register<IAppManager, UnitTestAppManager>();

            App.Settings = new AppSettings();
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
            var viewModel = new WeatherIconViewModel(() => Task.CompletedTask, description);

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
            var viewModel = new WeatherIconViewModel(() => Task.CompletedTask, description);
            Assert.IsTrue(viewModel.Tapped.CanExecute(null), "command must be able to executed");

            viewModel.Tapped.Execute(null);

            // check
            var appManager = DependencyService.Get<IAppManager>() as UnitTestAppManager;
            Assert.IsNotNull(appManager, "app manager must be non-null");
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
            var viewModel = new WeatherIconViewModel(() => Task.CompletedTask, description);
            Assert.IsTrue(viewModel.Tapped.CanExecute(null), "command must be able to executed");

            viewModel.Tapped.Execute(null);

            // can't check result
        }
    }
}
