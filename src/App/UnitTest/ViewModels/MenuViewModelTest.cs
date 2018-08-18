using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WhereToFly.App.Core;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class MenuViewModel
    /// </summary>
    [TestClass]
    public class MenuViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks and DependencyService with
        /// DataService.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
            DependencyService.Register<SvgImageCache>();

            var imageCache = DependencyService.Get<SvgImageCache>();
            imageCache.AddImage("icons/map.svg", string.Empty);
            imageCache.AddImage("icons/format-list-bulleted.svg", string.Empty);
            imageCache.AddImage("icons/map-marker-distance.svg", string.Empty);
            imageCache.AddImage("icons/compass.svg", string.Empty);
            imageCache.AddImage("icons/weather-partlycloudy.svg", string.Empty);
            imageCache.AddImage("icons/settings.svg", string.Empty);
            imageCache.AddImage("icons/information-outline.svg", string.Empty);
        }

        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // run
            var viewModel = new MenuViewModel();

            // check
            Assert.IsTrue(viewModel.MenuItemList.Any(), "menu item list must contain items");

            foreach (var menuItem in viewModel.MenuItemList)
            {
                var imageSource = menuItem.ImageSource;
                Assert.IsNotNull(imageSource, "image source must not be null");
            }
        }
    }
}
