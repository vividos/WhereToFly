using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using WhereToFly.App.Abstractions;
using WhereToFly.App.Logic;
using WhereToFly.App.Models;
using WhereToFly.App.Services.SqliteDatabase;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for WeatherImageCache class
    /// </summary>
    [TestClass]
    public class WeatherImageCacheTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IAppManager, UnitTestAppManager>();
            DependencyService.Register<IDataService, SqliteDatabaseDataService>();
        }

        /// <summary>
        /// Tests weather icon type IconPlaceholder
        /// </summary>
        /// <returns>task to wait for</returns>
        [TestMethod]
        public async Task TestIconTypePlaceholder()
        {
            // set up
            var placeholderIcon = new WeatherIconDescription
            {
                Name = "placeholder",
                Type = WeatherIconDescription.IconType.IconPlaceholder,
            };

            // run
            var imageSource = await WeatherImageCache.GetImageAsync(placeholderIcon);

            // check
            Assert.IsNotNull(imageSource, "returned image source must not be null");
        }

        /// <summary>
        /// Tests weather icon type IconApp
        /// </summary>
        /// <returns>task to wait for</returns>
        [TestMethod]
        public async Task TestIconTypeIconApp()
        {
            // run
            var imageSource = await WeatherImageCache.GetImageAsync(new WeatherIconDescription
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
            var desc = new WeatherIconDescription
            {
                Name = "hello world",
                Type = WeatherIconDescription.IconType.IconLink,
                WebLink = "https://localhost/test/123/",
            };

            // run
            var imageSource = await WeatherImageCache.GetImageAsync(desc);

            // check
            Assert.IsNotNull(imageSource, "returned image source must not be null");
        }

        /// <summary>
        /// Tests built-in icon for weather icon type IconLink
        /// </summary>
        /// <returns>task to wait for</returns>
        [TestMethod]
        public async Task TestBuiltInIconLink()
        {
            // set up
            var desc = new WeatherIconDescription
            {
                Name = "austrocontrol",
                Type = WeatherIconDescription.IconType.IconLink,
                WebLink = "https://www.austrocontrol.at",
            };

            // run
            var imageSource = await WeatherImageCache.GetImageAsync(desc);

            // check
            Assert.IsNotNull(imageSource, "returned image source must not be null");
        }
    }
}
