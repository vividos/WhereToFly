using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for WeatherImageCache class
    /// </summary>
    [TestClass]
    public class WeatherImageCacheTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks and app manager instance
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IAppManager, UnitTestAppManager>();
            DependencyService.Register<IDataService, DataService>();
        }

        /// <summary>
        /// Tests empty cache instance
        /// </summary>
        /// <returns>task to wait for</returns>
        [TestMethod]
        public async Task TestEmptyCache()
        {
            // set up
            var cache = new WeatherImageCache();

            // run
            var imageSource = await cache.GetImageAsync(new WeatherIconDescription
            {
                Name = "placeholder",
                Type = WeatherIconDescription.IconType.IconPlaceholder,
            });

            // check
            Assert.IsNotNull(imageSource, "returned image source must not be null");
        }

        /// <summary>
        /// Tests returning same image source instance
        /// </summary>
        /// <returns>task to wait for</returns>
        [TestMethod]
        public async Task TestCacheSameImageSource()
        {
            // set up
            var cache = new WeatherImageCache();

            // run
            var imageSource1 = await cache.GetImageAsync(new WeatherIconDescription
            {
                Name = "placeholder1",
                Type = WeatherIconDescription.IconType.IconPlaceholder,
            });

            var imageSource2 = await cache.GetImageAsync(new WeatherIconDescription
            {
                Name = "placeholder2",
                Type = WeatherIconDescription.IconType.IconPlaceholder,
            });

            // check
            Assert.IsNotNull(imageSource1, "returned image source 1 must not be null");
            Assert.IsNotNull(imageSource2, "returned image source 2 must not be null");

            Assert.AreEqual(imageSource1, imageSource2, "returned image sources must be equal");
        }

        /// <summary>
        /// Tests weather icon type IconApp
        /// </summary>
        /// <returns>task to wait for</returns>
        [TestMethod]
        public async Task TestIconTypeIconApp()
        {
            // set up
            var cache = new WeatherImageCache();

            // run
            var imageSource = await cache.GetImageAsync(new WeatherIconDescription
            {
                Name = "My App",
                Type = WeatherIconDescription.IconType.IconApp,
                WebLink = "de.myapp",
            });

            // check
            Assert.IsNotNull(imageSource, "returned image source must not be null");
        }

        /// <summary>
        /// Tests weather icon type IconLink
        /// </summary>
        /// <returns>task to wait for</returns>
        [TestMethod]
        public async Task TestIconTypeIconLink()
        {
            // set up
            var cache = new WeatherImageCache();

            // run
            var imageSource = await cache.GetImageAsync(new WeatherIconDescription
            {
                Name = "hello world",
                Type = WeatherIconDescription.IconType.IconLink,
                WebLink = "https://localhost/test/123/",
            });

            // check
            Assert.IsNotNull(imageSource, "returned image source must not be null");
        }
    }
}
