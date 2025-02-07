using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WhereToFly.App.Svg.UnitTest
{
    /// <summary>
    /// Unit tests for the <see cref="SvgImageSourceTypeConverter"/> class
    /// </summary>
    [TestClass]
    public class SvgImageSourceTypeConverterTests
    {
        /// <summary>
        /// Tests CanConvertFrom() method
        /// </summary>
        [TestMethod]
        public void TestCanConvertFrom()
        {
            // set up
            var converter = new SvgImageSourceTypeConverter();

            // run + test
            Assert.IsTrue(
                converter.CanConvertFrom(typeof(string)),
                "must be able to convert from string");

            Assert.IsFalse(
                converter.CanConvertFrom(typeof(ImageSource)),
                "must not be able to convert from ImageSource");
        }

        /// <summary>
        /// Tests converting null or whitespace string
        /// </summary>
        [TestMethod]
        public void TestConvertNullOrWhitespace()
        {
            // set up
            IExtendedTypeConverter converter = new SvgImageSourceTypeConverter();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // run + check
            object? imageSource1 = converter.ConvertFromInvariantString(null!, serviceProvider);
            object? imageSource2 = converter.ConvertFromInvariantString(string.Empty, serviceProvider);

            // check
            Assert.IsNull(imageSource1, "null value must be converted to null ImageSource");
            Assert.IsNull(imageSource2, "empty value must be converted to null ImageSource");
        }

        /// <summary>
        /// Tests converting from resource:// URI
        /// </summary>
        [TestMethod]
        public void TestConvertFromResourceUri()
        {
            // set up
            IExtendedTypeConverter converter = new SvgImageSourceTypeConverter();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // run
            string resourceUri = $"resource://{SvgTestImages.ResourcePathColibriSvg}";
            object? imageSource = converter.ConvertFromInvariantString(resourceUri, serviceProvider);

            // check
            Assert.IsNotNull(imageSource, "non-null ImageSource must have been returned");
            Assert.IsInstanceOfType<StreamImageSource>(imageSource, "must be a stream image source");
        }

        /// <summary>
        /// Tests converting from resource:// URI with assembly name
        /// </summary>
        [TestMethod]
        public void TestConvertFromResourceUriWithAssembly()
        {
            // set up
            IExtendedTypeConverter converter = new SvgImageSourceTypeConverter();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var assembly = typeof(SvgImageSourceTypeConverter).Assembly;

            // run
            string resourceUri = $"resource://{SvgTestImages.ResourcePathColibriSvg}?assembly={assembly.GetName().Name}";
            object? imageSource = converter.ConvertFromInvariantString(resourceUri, serviceProvider);

            // check
            Assert.IsNotNull(imageSource, "non-null ImageSource must have been returned");
            Assert.IsInstanceOfType<StreamImageSource>(imageSource, "must be a stream image source");
        }

        /// <summary>
        /// Tests converting from plain SVG xml string
        /// </summary>
        [TestMethod]
        public void TestConvertFromSvgImageText()
        {
            // set up
            IExtendedTypeConverter converter = new SvgImageSourceTypeConverter();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // run
            string svgImageText = SvgTestImages.TestSvgImageText;
            object? imageSource = converter.ConvertFromInvariantString(svgImageText, serviceProvider);

            // check
            Assert.IsNotNull(imageSource, "non-null ImageSource must have been returned");
            Assert.IsInstanceOfType<UriImageSource>(imageSource, "must be an URI image source");
        }

        /// <summary>
        /// Tests converting from data: URI containing a plain text SVG image
        /// </summary>
        [TestMethod]
        public void TestConvertFromDataUriPlainText()
        {
            // set up
            IExtendedTypeConverter converter = new SvgImageSourceTypeConverter();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // run
            string dataUri = SvgConstants.DataUriPlainPrefix + SvgTestImages.TestSvgImageText;
            object? imageSource = converter.ConvertFromInvariantString(dataUri, serviceProvider);

            // check
            Assert.IsNotNull(imageSource, "non-null ImageSource must have been returned");
            Assert.IsInstanceOfType<UriImageSource>(imageSource, "must be an URI image source");
        }

        /// <summary>
        /// Tests converting from data: URI containing a Base64 encoded SVG image
        /// </summary>
        [TestMethod]
        public void TestConvertFromDataUriBase64()
        {
            // set up
            IExtendedTypeConverter converter = new SvgImageSourceTypeConverter();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // run
            string dataUri = SvgConstants.DataUriBase64Prefix +
                SvgTestImages.EncodeBase64(SvgTestImages.TestSvgImageText);
            object? imageSource = converter.ConvertFromInvariantString(dataUri, serviceProvider);

            // check
            Assert.IsNotNull(imageSource, "non-null ImageSource must have been returned");
            Assert.IsInstanceOfType<UriImageSource>(imageSource, "must be an URI image source");
        }

        /// <summary>
        /// Tests converting from an invalid data: URI
        /// </summary>
        [TestMethod]
        public void TestConvertFromInvalidDataUri()
        {
            // set up
            IExtendedTypeConverter converter = new SvgImageSourceTypeConverter();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // run + check
            string dataUri = SvgConstants.DataUriPlainPrefix +
                new string('-', 65536);

            Assert.ThrowsException<FormatException>(
                () => converter.ConvertFromInvariantString(dataUri, serviceProvider),
                "must throw format exception on invalid URI");
        }

        /// <summary>
        /// Tests converting from file path
        /// </summary>
        [TestMethod]
        public void TestConvertFromFilename()
        {
            // set up
            IExtendedTypeConverter converter = new SvgImageSourceTypeConverter();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // run
            string filePath = "Assets/svg/colibri.svg";
            object? imageSource = converter.ConvertFromInvariantString(filePath, serviceProvider);

            // check
            Assert.IsNotNull(imageSource, "non-null ImageSource must have been returned");
            Assert.IsInstanceOfType<FileImageSource>(imageSource, "must be a file image source");
        }
    }
}
