using System;
using System.Diagnostics;
using System.Globalization;
using WhereToFly.App.Model;

namespace WhereToFly.App.Logic
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

        /// <summary>
        /// Formats distance value as displayable text
        /// </summary>
        /// <param name="distance">distance in meter</param>
        /// <returns>displayable text</returns>
        public static string FormatDistance(double distance)
        {
            if (distance < 1e-6)
            {
                return "-";
            }

            if (distance < 1000.0)
            {
                return string.Format("{0} m", (int)distance);
            }

            return string.Format("{0:F1} km", distance / 1000.0);
        }

        /// <summary>
        /// Formats text for sharing the current position with another app
        /// </summary>
        /// <param name="point">location map point</param>
        /// <param name="altitude">altitude in meters</param>
        /// <param name="dateTime">date time of position fix</param>
        /// <returns>displayable text for sharing</returns>
        public static string FormatMyPositionShareText(MapPoint point, double altitude, DateTimeOffset dateTime)
        {
            string mapsLink = string.Format(
                "https://www.google.com/maps/?q={0},{1}&z=15",
                point.Latitude.ToString("F6", CultureInfo.InvariantCulture),
                point.Longitude.ToString("F6", CultureInfo.InvariantCulture));

            return string.Format(
                "My current position is {0}, at an altitude of {1} m, as of {2} local time. {3}",
                point.ToString(),
                (int)altitude,
                dateTime.ToLocalTime().ToString("yyyy-MM-dd HH\\:mm\\:ss"),
                mapsLink);
        }
    }
}
