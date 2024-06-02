using System.Xml;
using WhereToFly.App.Logic;

namespace WhereToFly.App
{
    /// <summary>
    /// Markup extension to specify an image name (without extension) and the
    /// appropriate <see cref="ImageSource"/> is returned. Usage:
    /// {local:Image image_base_name}
    /// or
    /// {local:Image image_base_name.svg}
    /// </summary>
    [ContentProperty(nameof(BaseName))]
    public class ImageExtension : IMarkupExtension<ImageSource>
    {
        /// <summary>
        /// Image base name, without path to drawable or Assets folder. When without file
        /// extension, .png is appended, or when .svg was used, returns an SVG data URI image.
        /// </summary>
        public string? BaseName { get; set; }

        /// <summary>
        /// Called to provide an image source
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        /// <returns>image source</returns>
        public ImageSource ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(this.BaseName))
            {
                var lineInfoProvider =
                    serviceProvider.GetService<IXmlLineInfoProvider>();

                IXmlLineInfo lineInfo = (lineInfoProvider != null)
                    ? lineInfoProvider.XmlLineInfo
                    : new XmlLineInfo();

                throw new XamlParseException("ImageExtension requires Source property to be set", lineInfo);
            }

            if (this.BaseName.EndsWith(".svg"))
            {
                return SvgImageCache.GetImageSource(this.BaseName);
            }

            return ImageSource.FromFile(this.BaseName);
        }

        /// <summary>
        /// Called to provide a value; object version
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        /// <returns>value as object</returns>
        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<ImageSource>)
                .ProvideValue(serviceProvider);
        }
    }
}
