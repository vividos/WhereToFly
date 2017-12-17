using System;
using System.Diagnostics;
using System.Globalization;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core.Converter
{
    /// <summary>
    /// Converts MapShadingMode value to display text
    /// </summary>
    public class MapShadingModeDisplayConverter : IValueConverter
    {
        /// <summary>
        /// Converts MapShadingMode value to display text
        /// </summary>
        /// <param name="value">MapShadingMode value to convert</param>
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

            Debug.Assert(targetType == typeof(MapShadingMode), "target type must be MapShadingMode");

            var mapShadingMode = (MapShadingMode)value;

            switch (mapShadingMode)
            {
                case MapShadingMode.Fixed10Am:
                    return "Fixed at 10 a.m.";

                case MapShadingMode.Fixed3Pm:
                    return "Fixed at 3 p.m.";

                case MapShadingMode.CurrentTime:
                    return "Follow current time";

                case MapShadingMode.Ahead2Hours:
                    return "Current time + 2 hours";

                default:
                    Debug.Assert(false, "invalid map shading mode value");
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
