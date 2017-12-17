using System.Diagnostics;
using System.Globalization;
using WhereToFly.Logic.Model;

namespace WhereToFly.Logic
{
    /// <summary>
    /// Formatter for different data structures
    /// </summary>
    public static class DataFormatter
    {
        /// <summary>
        /// Formats latitude and longitude values
        /// </summary>
        /// <param name="latLong">latitude or longitude value</param>
        /// <param name="format">coordinate display format</param>
        /// <returns>formatted latitude or longitude value</returns>
        public static string FormatLatLong(double latLong, CoordinateDisplayFormat format)
        {
            switch (format)
            {
                case CoordinateDisplayFormat.Format_dd_dddddd:
                    return latLong.ToString("F6", CultureInfo.InvariantCulture);

                case CoordinateDisplayFormat.Format_dd_mm_mmm:
                    double fractionalPart = (latLong - (int)latLong) * 60.0;
                    return string.Format(CultureInfo.InvariantCulture, "{0}° {1:F3}'", (int)latLong, fractionalPart);

                case CoordinateDisplayFormat.Format_dd_mm_sss:
                    double minutePart = (latLong - (int)latLong) * 60.0;
                    double secondsPart = (minutePart - (int)minutePart) * 60.0;
                    return string.Format(CultureInfo.InvariantCulture, "{0}° {1}' {2}\"", (int)latLong, (int)minutePart, (int)secondsPart);

                default:
                    Debug.Assert(false, "invalid coordinate display format");
                    break;
            }

            return "?";
        }
    }
}
