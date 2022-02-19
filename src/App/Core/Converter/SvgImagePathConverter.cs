using System;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Converter
{
    /// <summary>
    /// Converts a path to an svg image to an SvgImageSource. Use
    /// like this:
    /// Icon="{Binding Source={StaticResource ImageBaseName}, Converter={StaticResource SvgImagePathConverter}}"
    /// Before that, declare a string in the Resources of the page:
    /// <![CDATA[
    /// <ContentPage.Resources>
    ///   <ResourceDictionary>
    ///     <converter:SvgImagePathConverter x:Key="SvgImagePathConverter" />
    ///     <x:String x:Key="ImageBaseName">xxx</x:String>
    ///   </ResourceDictionary>
    /// </ContentPage.Resources>
    /// ]]>
    /// </summary>
    public class SvgImagePathConverter : IValueConverter
    {
        /// <summary>
        /// Converts svg image path to image source.
        /// </summary>
        /// <param name="value">value to use</param>
        /// <param name="targetType">target type to convert to</param>
        /// <param name="parameter">parameter to use, from ConverterParameter</param>
        /// <param name="culture">specific culture to use; unused</param>
        /// <returns>boolean true value when value greater-or-equal parameter, false else</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(
                targetType == typeof(ImageSource),
                "convert target must be an ImageSource");

            string svgImageName = (string)value;

            return SvgImageCache.GetImageSource(svgImageName);
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
