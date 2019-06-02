using System;
using System.Diagnostics;
using System.Globalization;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;

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
                    double fractionalPart = Math.Abs((latLong - (int)latLong) * 60.0);
                    return string.Format(CultureInfo.InvariantCulture, "{0}° {1:F3}'", (int)latLong, fractionalPart);

                case CoordinateDisplayFormat.Format_dd_mm_sss:
                    double minutePart = Math.Abs((latLong - (int)latLong) * 60.0);
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
        /// <param name="distanceInMeter">distance in meter</param>
        /// <returns>displayable text</returns>
        public static string FormatDistance(double distanceInMeter)
        {
            if (distanceInMeter < 1e-6)
            {
                return "-";
            }

            if (distanceInMeter < 1000.0)
            {
                return string.Format("{0} m", (int)distanceInMeter);
            }

            return string.Format("{0:F1} km", distanceInMeter / 1000.0);
        }

        /// <summary>
        /// Formats duration, for display
        /// </summary>
        /// <param name="duration">duration to format</param>
        /// <returns>formatted duration</returns>
        public static string FormatDuration(TimeSpan duration)
        {
            return string.Format(duration >= TimeSpan.FromDays(1) ? "{0:d\\.hh\\:mm\\:ss}" : "{0:h\\:mm\\:ss}", duration);
        }

        /// <summary>
        /// Formats text for sharing the current position with another app
        /// </summary>
        /// <param name="point">location map point</param>
        /// <param name="dateTime">date time of position fix</param>
        /// <returns>displayable text for sharing</returns>
        public static string FormatMyPositionShareText(MapPoint point, DateTimeOffset dateTime)
        {
            string mapsLink = string.Format(
                "https://www.google.com/maps/?q={0},{1}&z=15",
                point.Latitude.ToString("F6", CultureInfo.InvariantCulture),
                point.Longitude.ToString("F6", CultureInfo.InvariantCulture));

            return string.Format(
                "My current position is Lat={0}, Long={1}, at an altitude of {2} m, as of {3} local time. {4}",
                point.Latitude.ToString("F6", CultureInfo.InvariantCulture),
                point.Longitude.ToString("F6", CultureInfo.InvariantCulture),
                (int)point.Altitude.GetValueOrDefault(0.0),
                dateTime.ToLocalTime().ToString("yyyy-MM-dd HH\\:mm\\:ss"),
                mapsLink);
        }

        /// <summary>
        /// Formats text for sharing a location with another app
        /// </summary>
        /// <param name="location">location to share</param>
        /// <returns>displayable text for sharing</returns>
        public static string FormatLocationShareText(Location location)
        {
            return string.Format(
                "Here is a location named \"{0}\", at coordinates Lat={1}, Long={2} and altitude of {3} m.",
                location.Name,
                location.MapLocation.Latitude.ToString("F6", CultureInfo.InvariantCulture),
                location.MapLocation.Longitude.ToString("F6", CultureInfo.InvariantCulture),
                (int)location.MapLocation.Altitude.GetValueOrDefault(0.0));
        }
    }
}
