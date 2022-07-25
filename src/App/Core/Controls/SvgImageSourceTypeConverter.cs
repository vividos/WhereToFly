using System;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

#nullable enable

namespace WhereToFly.App.Core.Controls
{
    /// <summary>
    /// Type converter for SVG based image sources; takes a string representation of an SVG image
    /// resource and returns a suitable ImageSource object. The following syntaxes are supported:
    /// resource://ProjectName.Path.To.Resource.Image.svg
    /// resource://ProjectName.Path.To.Resource.Image.svg?assembly=FullAssemblyName
    /// data:image/svg+xml,SvgText
    /// data:image/svg+xml;base64,Base64EncodedSvgText
    /// </summary>
    [TypeConversion(typeof(ImageSource))]
    public sealed class SvgImageSourceTypeConverter : TypeConverter
    {
        /// <summary>
        /// Returns which types can be converted
        /// </summary>
        /// <param name="sourceType">source type</param>
        /// <returns>true when type can be converted, or false when not</returns>
        public override bool CanConvertFrom(Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Converts a string value to a suitable ImageSource object.
        /// </summary>
        /// <param name="value">string value to convert</param>
        /// <returns>converted ImageSource object</returns>
        public override object? ConvertFromInvariantString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string text = value;

            if (text.StartsWith(SvgConstants.ResourceUriPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return ParseResourceUri(text);
            }

            if (text.StartsWith("<svg", StringComparison.InvariantCultureIgnoreCase))
            {
                text = text.Insert(0, SvgConstants.DataUriPlainPrefix);
            }

            if (text.StartsWith(SvgConstants.DataUriPlainPrefix, StringComparison.InvariantCultureIgnoreCase) ||
                text.StartsWith(SvgConstants.DataUriBase64Prefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return ParseDataUri(text);
            }

            return ImageSource.FromFile(value);
        }

        /// <summary>
        /// Parses resource:// URI and returns suitable ImageResource instance
        /// </summary>
        /// <param name="resourceUri">resource URI to parse</param>
        /// <returns>resource image source</returns>
        private static ImageSource ParseResourceUri(string resourceUri)
        {
            resourceUri = resourceUri.Substring(SvgConstants.ResourceUriPrefix.Length);

            Assembly? sourceAssembly = null;

            int pos = resourceUri.IndexOf(SvgConstants.ResourceUriAssemblyQuery);
            if (pos != -1)
            {
                string assemblyName = resourceUri.Substring(
                    pos + SvgConstants.ResourceUriAssemblyQuery.Length);

                sourceAssembly = Assembly.Load(new AssemblyName(assemblyName));

                resourceUri = resourceUri.Substring(0, pos);
            }

            if (sourceAssembly == null)
            {
                sourceAssembly = Assembly.GetCallingAssembly();
            }

            return ImageSource.FromResource(resourceUri, sourceAssembly);
        }

        /// <summary>
        /// Parses data: URI and returns suitable ImageResource instance
        /// </summary>
        /// <param name="dataUri">resource URI to parse</param>
        /// <returns>data URI image source</returns>
        private static ImageSource ParseDataUri(string dataUri)
        {
            if (Uri.TryCreate(dataUri, UriKind.Absolute, out Uri uri))
            {
                return ImageSource.FromUri(uri);
            }
            else
            {
                throw new FormatException("SVG image source: invalid data URI: " + dataUri);
            }
        }
    }
}
