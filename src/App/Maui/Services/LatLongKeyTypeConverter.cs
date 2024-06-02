using System.ComponentModel;
using System.Globalization;

namespace WhereToFly.App.Services
{
    /// <summary>
    /// Type converter to convert LatLongKey type from/to string
    /// </summary>
    internal class LatLongKeyTypeConverter : TypeConverter
    {
        /// <summary>
        /// Returns if a LatLongKey can be converted from the given source type
        /// </summary>
        /// <param name="context">context; unused</param>
        /// <param name="sourceType">source type</param>
        /// <returns>true when source type is convertible</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(string);

        /// <summary>
        /// Returns if a LatLongKey can be converted to the given destination type
        /// </summary>
        /// <param name="context">context; unused</param>
        /// <param name="destinationType">destination type</param>
        /// <returns>true when destination type is convertible</returns>
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
            => destinationType == typeof(string);

        /// <summary>
        /// Converts from string to LatLongKey
        /// </summary>
        /// <param name="context">context; unused</param>
        /// <param name="culture">culture; unused</param>
        /// <param name="value">string value to convert</param>
        /// <returns>converted LatLongKey object</returns>
        public override object? ConvertFrom(
            ITypeDescriptorContext? context,
            CultureInfo? culture,
            object value)
        {
            if (value is string text)
            {
                int posComma = text.IndexOf(',');
                string latitude = text.Substring(0, posComma);
                string longitude = text.Substring(posComma + 1);

                return new LatLongKey(int.Parse(latitude), int.Parse(longitude));
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Converts from LatLongKey to string
        /// </summary>
        /// <param name="context">context; unused</param>
        /// <param name="culture">culture; unused</param>
        /// <param name="value">LatLongKey value to convert</param>
        /// <param name="destinationType">destination type</param>
        /// <returns>converted string</returns>
        public override object? ConvertTo(
            ITypeDescriptorContext? context,
            CultureInfo? culture,
            object? value,
            Type destinationType)
        {
            if (value is LatLongKey key &&
                destinationType == typeof(string))
            {
                return $"{key.Latitude},{key.Longitude}";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
