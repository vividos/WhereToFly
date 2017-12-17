using System;
using System.Diagnostics;
using System.Globalization;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core.Converter
{
    /// <summary>
    /// Converts CoordinateDisplayFormat value to display text
    /// </summary>
    public class CoordinateDisplayFormatDisplayConverter : IValueConverter
    {
        /// <summary>
        /// Converts CoordinateDisplayFormat value to display text
        /// </summary>
        /// <param name="value">CoordinateDisplayFormat value to convert</param>
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

            Debug.Assert(targetType == typeof(CoordinateDisplayFormat), "target type must be CoordinateDisplayFormat");

            var format = (CoordinateDisplayFormat)value;

            switch (format)
            {
                case CoordinateDisplayFormat.Format_dd_dddddd:
                    return "dd.dddddd°";

                case CoordinateDisplayFormat.Format_dd_mm_mmm:
                    return "dd° mm.mmm'";
                case CoordinateDisplayFormat.Format_dd_mm_sss:
                    return "dd° mm' sss\"";

                default:
                    Debug.Assert(false, "invalid format value");
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
