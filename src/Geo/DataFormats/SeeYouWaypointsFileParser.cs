using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo.DataFormats
{
    /// <summary>
    /// File parser for SeeYou waypoints file with extension .cup. See .cup specification:
    /// https://download.naviter.com/docs/CUP-file-format-description.pdf
    /// </summary>
    internal class SeeYouWaypointsFileParser
    {
        /// <summary>
        /// Separator line between waypoints and task definition
        /// </summary>
        private const string TaskSeparator = "-----Related Tasks-----";

        /// <summary>
        /// File stream
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// Creates a new file parser for SeeYou waypoints files
        /// </summary>
        /// <param name="stream">stream to use</param>
        public SeeYouWaypointsFileParser(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Parses the waypoints file and returns locations
        /// </summary>
        /// <returns>enumerable of locations</returns>
        public IEnumerable<Location> Parse()
        {
            bool isFirstLine = true;
            using var reader = new StreamReader(this.stream);
            do
            {
                string line = reader.ReadLine();
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                if (line.Contains(TaskSeparator))
                {
                    yield break;
                }

                string[] parts = SplitWithQuotes(line).ToArray();

                Location location = LocationFromParts(parts);
                if (location != null)
                {
                    yield return location;
                }
            }
            while (!reader.EndOfStream);
        }

        /// <summary>
        /// Splits a line by the comma separator character, but keeps strings in quotes together.
        /// </summary>
        /// <param name="line">line to parse</param>
        /// <returns>split parts of the line</returns>
        private static IEnumerable<string> SplitWithQuotes(string line)
        {
            bool inQuote = false;

            int currentStart = 0;

            for (int currentIndex = 0; currentIndex < line.Length; currentIndex++)
            {
                char ch = line[currentIndex];
                if (ch == '"')
                {
                    inQuote = !inQuote;
                }

                if (ch == ',' && !inQuote)
                {
                    yield return line.Substring(currentStart, currentIndex - currentStart).Trim('"');
                    currentStart = currentIndex + 1;
                }
            }

            yield return line.Substring(currentStart).Trim('"');
        }

        /// <summary>
        /// Parses data in the split parts of a line and returns a location
        /// </summary>
        /// <param name="parts">parts text strings of a line</param>
        /// <returns>newly created location, or null when parts couldn't be parsed</returns>
        private static Location LocationFromParts(string[] parts)
        {
#pragma warning disable S125 // Sections of code should not be "commented out"
            // Waypoint name
            // |
            // |       Code or short name
            // |       |
            // |       |      Country IANA top level domain
            // |       |      |
            // |       |      |  Latitude; format DDMM.mmmX with X = {N,S}
            // |       |      |  |
            // |       |      |  |         Longitude; format DDDMM.mmmX with X = {E,W}
            // |       |      |  |         |
            // |       |      |  |         |          Elevation, decimal separator, unit {m,ft}
            // |       |      |  |         |          |
            // |       |      |  |         |          |      Waypoint style (number)
            // |       |      |  |         |          |      |
            // |       |      |  |         |          |      | Runway direction (degrees)
            // |       |      |  |         |          |      | |
            // |       |      |  |         |          |      | |   Runway length, decimal separator, unit {m,nm,ml}
            // |       |      |  |         |          |      | |   |
            // |       |      |  |         |          |      | |   |       Airport frequency
            // |       |      |  |         |          |      | |   |       |       Description
            // |       |      |  |         |          |      | |   |       |       |
            // "Lesce","LJBL",SI,4621.379N,01410.467E,504.0m,5,144,1130.0m,123.500,"Home Airfield"
#pragma warning restore S125

            if (parts == null || parts.Length != 11)
            {
                return null;
            }

            return new Location
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = parts[0],
                MapLocation = new MapPoint(
                    ParseLatLong("0" + parts[3]),
                    ParseLatLong(parts[4]),
                    ParseAltitude(parts[5])),
                Description = parts[10],
                Type = LocationTypeFromWaypointStyle(parts[7]),
            };
        }

        /// <summary>
        /// Parses latitude or longitude string value, in the format DDDMMmmmX, with DDD the
        /// degrees part, MM the minutes part, mmm the fractional minutes part and X the
        /// direction, either N, S, E or W.
        /// As latitude has only 2 digits for DDD, prepend it with digit 0 before passing to this
        /// method.
        /// </summary>
        /// <param name="latLong">latitude or longitude value as text</param>
        /// <returns>parsed latitude or longitude value</returns>
        private static double ParseLatLong(string latLong)
        {
            if (latLong.Length != 10)
            {
                return 0.0;
            }

            int decimalValue = Convert.ToInt32(latLong.Substring(0, 3));
            int minuteValue = Convert.ToInt32(latLong.Substring(3, 2));
            int minuteFractional = Convert.ToInt32(latLong.Substring(6, 3));

            double value = decimalValue + ((minuteValue + (minuteFractional / 1000.0)) / 60.0);

            char direction = latLong[9];
            if (direction == 'S' || direction == 'W')
            {
                value = -value;
            }

            return value;
        }

        /// <summary>
        /// Parses the altitude text with possible decimal separater and unit in meter or feet.
        /// </summary>
        /// <param name="altitudeText">altitude text</param>
        /// <returns>altitude value in meter, or null when value couldn't be parsed</returns>
        private static double? ParseAltitude(string altitudeText)
        {
            string localAltitudeText;
            bool convertFromFeet = false;

            if (altitudeText.EndsWith("m"))
            {
                localAltitudeText = altitudeText.TrimEnd('m');
            }
            else if (altitudeText.EndsWith("ft"))
            {
                localAltitudeText = altitudeText.Substring(0, altitudeText.Length - 2);
                convertFromFeet = true;
            }
            else
            {
                return null;
            }

            double altitude = Convert.ToDouble(
                localAltitudeText,
                System.Globalization.CultureInfo.InvariantCulture);

            return convertFromFeet ? altitude * Constants.FactorFeetToMeter : altitude;
        }

        /// <summary>
        /// Returns a location type based on the waypoint style value from a .cup file
        /// </summary>
        /// <param name="style">waypoint style as text</param>
        /// <returns>location type</returns>
        private static LocationType LocationTypeFromWaypointStyle(string style)
        {
            if (string.IsNullOrWhiteSpace(style))
            {
                return LocationType.Waypoint;
            }

            // the values are given in the specification linked in this classes summary
            return Convert.ToInt32(style) switch
            {
                2 or 3 or 5 => LocationType.FlyingLandingPlace,
                4 => LocationType.FlyingTakeoff,
                6 => LocationType.Pass,
                7 => LocationType.Summit,
                14 => LocationType.Bridge,
                _ => LocationType.Waypoint,
            };
        }
    }
}
