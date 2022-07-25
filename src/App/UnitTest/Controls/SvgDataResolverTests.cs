using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using WhereToFly.App.Core.Controls;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.Controls
{
    /// <summary>
    /// Unit tests for the SvgDataResolver class
    /// </summary>
    [TestClass]
    public class SvgDataResolverTests
    {
        /// <summary>
        /// Sets up unit tests by initializing Xamarin.Forms
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
        }

        /// <summary>
        /// Tests calling LoadSvgImage() with a null image source; this must result in a null
        /// SKSvg instance being returned. No exception is thrown.
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestLoadSvgImage_NullArgument()
        {
            // run
            var svg = await SvgDataResolver.LoadSvgImage(null);

            // check
            Assert.IsNull(svg, "loaded SKSvg object must be null");
        }

        /// <summary>
        /// Tests error when stream factory action is null
        /// </summary>
        [TestMethod]
        public void TestStreamImageSource_StreamFactoryIsNull()
        {
            // set up
            var imageSource = new StreamImageSource
            {
                Stream = null
            };

            // run
            Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await SvgDataResolver.LoadSvgImage(imageSource),
                "specifying null Stream factory must throw an exception");
        }

        /// <summary>
        /// Tests error when stream factory action produces a Stream.Null object
        /// </summary>
        [TestMethod]
        public void TestStreamImageSource_StreamFactoryProducesNullStream()
        {
            // set up
            var imageSource = ImageSource.FromStream(
                (ct) => Task.FromResult(Stream.Null));

            // run
            Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await SvgDataResolver.LoadSvgImage(imageSource),
                "specifying Stream.Null must throw an exception");
        }

        /// <summary>
        /// Tests error when stream factory action produces a null Stream object
        /// </summary>
        [TestMethod]
        public void TestStreamImageSource_StreamFactoryProducesNullObject()
        {
            // set up
            var imageSource = ImageSource.FromStream(
                (ct) => Task.FromResult<Stream>(null));

            // run + check
            Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await SvgDataResolver.LoadSvgImage(imageSource),
                "null Stream object must throw an exception");
        }

        /// <summary>
        /// Tests loading SVG image from MemoryStream object
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestStreamImageSource_MemoryStream()
        {
            // set up
            var imageBytes = System.Text.Encoding.UTF8.GetBytes(
                SvgTestImages.TestSvgImageText);

            var imageSource = ImageSource.FromStream(
                (ct) => Task.FromResult<Stream>(new MemoryStream(imageBytes)));

            // run
            var svg = await SvgDataResolver.LoadSvgImage(imageSource);

            // check
            Assert.IsNotNull(svg, "loaded SKSvg object must be non-null");
            Assert.IsNotNull(svg.Picture, "SKPicture object must be non-null");
            Assert.IsNotNull(svg.Picture.CullRect, "CullRect must be non-null");
            Assert.IsFalse(svg.Picture.CullRect.IsEmpty, "CullRect must not be empty");
        }

        /// <summary>
        /// Tests loading an SVG image using a data: URI containing a plain text SVG image
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestUriImageSource_DataUriPlainText()
        {
            // set up
            var uri = new Uri(SvgConstants.DataUriPlainPrefix + SvgTestImages.TestSvgImageText);
            var imageSource = ImageSource.FromUri(uri);

            // run
            var svg = await SvgDataResolver.LoadSvgImage(imageSource);

            // check
            Assert.IsNotNull(svg, "loaded SKSvg object must be non-null");
            Assert.IsNotNull(svg.Picture, "SKPicture object must be non-null");
            Assert.IsNotNull(svg.Picture.CullRect, "CullRect must be non-null");
            Assert.IsFalse(svg.Picture.CullRect.IsEmpty, "CullRect must not be empty");
        }

        /// <summary>
        /// Tests loading an SVG image using a data: URI containing a Base64 encoded SVG image
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestUriImageSource_DataUriBase64()
        {
            // set up
            var uri = new Uri(SvgConstants.DataUriBase64Prefix +
                SvgTestImages.EncodeBase64(SvgTestImages.TestSvgImageText));
            var imageSource = ImageSource.FromUri(uri);

            // run
            var svg = await SvgDataResolver.LoadSvgImage(imageSource);

            // check
            Assert.IsNotNull(svg, "loaded SKSvg object must be non-null");
            Assert.IsNotNull(svg.Picture, "SKPicture object must be non-null");
            Assert.IsNotNull(svg.Picture.CullRect, "CullRect must be non-null");
            Assert.IsFalse(svg.Picture.CullRect.IsEmpty, "CullRect must not be empty");
        }

        /// <summary>
        /// Tests loading an SVG image using a data: URI containing invalid SVG image text
        /// </summary>
        [TestMethod]
        public void TestUriImageSource_DataUriInvalidText()
        {
            // set up
            var uri = new Uri("data:image/abc+xml;base65,aaaaaaaaaa");
            var imageSource = ImageSource.FromUri(uri);

            // run + check
            Assert.ThrowsExceptionAsync<FormatException>(
                async () => await SvgDataResolver.LoadSvgImage(imageSource),
                "invalid data: URI must throw an exception");
        }

        /// <summary>
        /// Tests loading an SVG image using an invalid or not supported URI scheme
        /// </summary>
        [TestMethod]
        public void TestUriImageSource_InvalidUriScheme()
        {
            // set up
            var uri = new Uri("https://raw.githubusercontent.com/vividos/WhereToFly/main/src/App/UnitTest/Assets/svg/toucan.svg");
            var imageSource = ImageSource.FromUri(uri);

            // run + check
            Assert.ThrowsExceptionAsync<NotSupportedException>(
                async () => await SvgDataResolver.LoadSvgImage(imageSource),
                "invalid URI schema must throw an exception");
        }

        /// <summary>
        /// Tests loading an SVG image from a file stored in the Assets folder
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestFileImageSource_FileImageSource()
        {
            var testPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
            var imageSource = ImageSource.FromFile(
                Path.Combine(testPath, "Assets/svg/cog-outline.svg"));

            // run
            var svg = await SvgDataResolver.LoadSvgImage(imageSource);

            // check
            Assert.IsNotNull(svg, "loaded SKSvg object must be non-null");
            Assert.IsNotNull(svg.Picture, "SKPicture object must be non-null");
            Assert.IsNotNull(svg.Picture.CullRect, "CullRect must be non-null");
            Assert.IsFalse(svg.Picture.CullRect.IsEmpty, "CullRect must not be empty");
        }
    }
}
