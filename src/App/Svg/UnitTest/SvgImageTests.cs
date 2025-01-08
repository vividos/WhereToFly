using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WhereToFly.App.Svg.UnitTest
{
    /// <summary>
    /// Unit tests for the SvgImage class
    /// </summary>
    [TestClass]
    public class SvgImageTests
    {
        /// <summary>
        /// Resource URI for a valid image to use for testing
        /// </summary>
        private const string ResourceUri =
            "resource://WhereToFly.App.UnitTest.Assets.svg.colibri.svg?assembly=WhereToFly.App.UnitTest";

        /// <summary>
        /// Sets up unit tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            MauiMocks.Init();
        }

        /// <summary>
        /// Cleans up unit tests
        /// </summary>
        [TestCleanup]
        public void TearDown()
        {
            MauiMocks.Reset();
        }

        /// <summary>
        /// Tests SvgImage default ctor
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // run
            var image = new SvgImage();

            // check
            Assert.IsNull(image.Source, "default constructed SvgImage object must have null Source");
        }

        /// <summary>
        /// Tests setting image source
        /// </summary>
        [TestMethod]
        public void TestSetImageSource()
        {
            // set up
            var image = new SvgImage();

            var uri = new Uri(ResourceUri);
            var imageSource = ImageSource.FromUri(uri);

            // run
            image.Source = imageSource;

            // check
            Assert.IsNotNull(image.Source, "image source must have been set");
        }

        /// <summary>
        /// Tests error while setting image source
        /// </summary>
        [TestMethod]
        public void TestErrorSettingImageSource()
        {
            // set up
            var image = new SvgImage();

            var imageSource = ImageSource.FromStream(
                (ct) => Task.FromResult<Stream?>(null));

            // run
            image.Source = imageSource;

            // check
            Assert.IsNotNull(image.Source, "image source must have been set");
        }

        /// <summary>
        /// Tests setting tint color
        /// </summary>
        [TestMethod]
        public void TestSetTintColor()
        {
            // set up
            var image = new SvgImage();

            var uri = new Uri(ResourceUri);
            var imageSource = ImageSource.FromUri(uri);
            image.Source = imageSource;

            // run
            image.TintColor = Colors.Goldenrod;

            // check
            Assert.AreEqual(Colors.Goldenrod, image.TintColor, "tint color must match");
        }
    }
}
