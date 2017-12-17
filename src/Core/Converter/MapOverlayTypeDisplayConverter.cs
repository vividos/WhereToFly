using System;
using System.Diagnostics;
using System.Globalization;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core.Converter
{
    /// <summary>
    /// Converts MapOverlayType value to display text
    /// </summary>
    public class MapOverlayTypeDisplayConverter : IValueConverter
    {
        /// <summary>
        /// Converts MapOverlayType value to display text
        /// </summary>
        /// <param name="value">MapOverlayType value to convert</param>
        /// <param name="targetType">target type; must be string</param>
        /// <param name="parameter">parameter to use; unused</param>
        /// <param name="culture">specific culture to use; unused</param>
        /// <returns>converted value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            Debug.Assert(targetType == typeof(MapOverlayType), "target type must be MapOverlayType");

            var mapOverlayType = (MapOverlayType)value;

            switch (mapOverlayType)
            {
                case MapOverlayType.OpenStreetMap:
                    return "OpenStreetMap";

                default:
                    Debug.Assert(false, "invalid map overlay type value");
                    break;
            }

            return "???";
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
