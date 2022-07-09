using System;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Converter
{
    /// <summary>
    /// Converts image base path to an actual image path, suitable for the runtime platform. Use
    /// like this:
    /// Icon="{Binding Source={StaticResource ImageBaseName}, Converter={StaticResource ImagePathConverter}}"
    /// Before that, declare a string in the Resources of the page:
    /// <![CDATA[
    /// <ContentPage.Resources>
    ///   <ResourceDictionary>
    ///     <converter:ImagePathConverter x:Key="ImagePathConverter" />
    ///     <x:String x:Key="ImageBaseName">xxx</x:String>
    ///   </ResourceDictionary>
    /// </ContentPage.Resources>
    /// ]]>
    /// </summary>
    public class ImagePathConverter : IValueConverter
    {
        /// <summary>
        /// Gets the image path for a specific device, by using image base name and adding
        /// necessary paths and extensions. The path is suitable for ToolbarItem, MenuItem and
        /// Image elements.
        /// </summary>
        /// <param name="imageBaseName">image base name</param>
        /// <returns>image path</returns>
        public static string GetDeviceDependentImage(string imageBaseName)
        {
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    return imageBaseName + ".xml";

                case Device.UWP:
                case Device.iOS:
                    return $"Assets/images/{imageBaseName}.png";

                case "Test": // returned by Xamarin.Forms.Mocks
                    return imageBaseName;

                default:
                    Debug.Assert(false, "invalid runtime platform");
                    return string.Empty;
            }
        }

        /// <summary>
        /// Converts image base path to device dependent image path.
        /// </summary>
        /// <param name="value">value to use</param>
        /// <param name="targetType">target type to convert to</param>
        /// <param name="parameter">parameter to use, from ConverterParameter</param>
        /// <param name="culture">specific culture to use; unused</param>
        /// <returns>boolean true value when value greater-or-equal parameter, false else</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(
                targetType == typeof(string) ||
                targetType == typeof(FileImageSource) ||
                targetType == typeof(ImageSource),
                "convert target must be string, FileImageSource or ImageSource");

            string imageBaseName = (string)value;

            string imagePath = GetDeviceDependentImage(imageBaseName);

            if (targetType == typeof(FileImageSource))
            {
                return new FileImageSource
                {
                    File = imagePath,
                };
            }

            if (targetType == typeof(ImageSource))
            {
                return ImageSource.FromFile(imagePath);
            }

            return imagePath;
        }

        /// <summary>
        /// Converts back; not implemented
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">parameter to use; unused</param>
        /// <param name="culture">specific culture to use; unused</param>
        /// <returns>converted value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
