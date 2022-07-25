using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using WhereToFly.App.Core.Controls;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.Controls
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
        /// Sets up unit tests by initializing Xamarin.Forms
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
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
                (ct) => Task.FromResult<Stream>(null));

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
            image.TintColor = Color.Goldenrod;

            // check
            Assert.AreEqual(Color.Goldenrod, image.TintColor, "tint color must match");
        }
    }
}
