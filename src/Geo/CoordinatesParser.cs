using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo
{
    /// <summary>
    /// Parser for a wide range of coordinate formats, mostly for WGS84.
    /// </summary>
    public static class CoordinatesParser
    {
        /// <summary>
        /// Tries parsing a text string and finding latitude and longitude values.
        /// </summary>
        /// <param name="text">text to parse</param>
        /// <param name="mapPoint">
        /// map point, containing parsed values of latitude and longitude
        /// </param>
        /// <returns>true when parsing succeeded, or false when not</returns>
        public static bool TryParse(string? text, out MapPoint? mapPoint)
        {
            mapPoint = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text
                .Replace('\t', ' ')
                .Replace('\n', ' ')
                .Replace('\r', ' ')
                .Replace('\u202f', ' ');

            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }

            text = text.Trim();

            // try parse as URI
            if (text.StartsWith("http://", ignoreCase: true, null) ||
                text.StartsWith("https://", ignoreCase: true, null) ||
                text.StartsWith("geo:", ignoreCase: true, null))
            {
                return TryParseUri(text, out mapPoint);
            }

            text = text
                .Replace("′", "'")
                .Replace("″", "\"")
                .Replace("’", "'")
                .Replace("”", "\"")
                .Replace("° ", "°")
                .Replace("' ", "'")
                .Replace("\" ", "\"")
                .Replace("''", "\"");

            // try to split the text into two parts
            if (!TrySplitCoordinates(text, out string? latitude, out string? longitude) ||
                latitude == null ||
                longitude == null)
            {
                return false;
            }

            if (TryParseDecimals(latitude, longitude, out mapPoint))
            {
                return true;
            }

            if (TryParseDegreesMinutesSeconds(latitude, longitude, out mapPoint))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to split text into latitude and longitude
        /// </summary>
        /// <param name="text">coordinates text to split</param>
        /// <param name="latitude">latitude text</param>
        /// <param name="longitude">longitude text</param>
        /// <returns>true when splitting succeeded, or false when not</returns>
        private static bool TrySplitCoordinates(string text, out string? latitude, out string? longitude)
        {
            latitude = null;
            longitude = null;

            int posCommaSpace = text.IndexOf(", ");
            if (posCommaSpace != -1 &&
                posCommaSpace == text.LastIndexOf(", "))
            {
                text = text.Replace(", ", ",");
            }

            int splitPos = -1;

            int countSpaces = text.Count(c => c == ' ');
            int countCommas = text.Count(c => c == ',');
            int countSemicolon = text.Count(c => c == ';');

            if (countSpaces == 1)
            {
                splitPos = text.IndexOf(' ');
            }
            else if (countCommas == 1)
            {
                splitPos = text.IndexOf(',');
            }
            else if (countCommas == 3)
            {
                splitPos = text.IndexOf(',', text.IndexOf(',') + 1);
                text = text.Replace(',', '.');
            }
            else if (countSemicolon == 1)
            {
                splitPos = text.IndexOf(';');
            }

            int numCharsToRemove = 1;

            if (splitPos == -1)
            {
                splitPos = FindSplitPosByDirectionChar(text);
                numCharsToRemove = 0;
            }

            if (splitPos == -1)
            {
                return false;
            }

            latitude = text.Substring(0, splitPos);
            longitude = text.Substring(splitPos + numCharsToRemove).TrimStart();

            latitude = latitude.Replace(" ", string.Empty).Trim();
            longitude = longitude.Replace(" ", string.Empty).Trim();

            CheckSwapLatLong(ref latitude, ref longitude);

            return true;
        }

        /// <summary>
        /// Finds split position based on latitude / longitude direction characters
        /// </summary>
        /// <param name="text">coordinates text to split</param>
        /// <returns>
        /// split position, or -1 when no position was found
        /// </returns>
        private static int FindSplitPosByDirectionChar(string text)
        {
            bool startsWithLatitudeDirectionCharacter =
                Array.Exists(LatitudeDirectionCharacter, text.StartsWith);

            bool startsWithLongitudeDirectionCharacter =
                Array.Exists(LongitudeDirectionCharacter, text.StartsWith);

            if (!startsWithLatitudeDirectionCharacter &&
                !startsWithLongitudeDirectionCharacter)
            {
                return -1;
            }

            if (startsWithLatitudeDirectionCharacter)
            {
                return
                    LongitudeDirectionCharacter
                    .Where(text.Contains)
                    .Select(ch => text.IndexOf(ch))
                    .FirstOrDefault(-1);
            }

            if (startsWithLongitudeDirectionCharacter)
            {
                return
                    LatitudeDirectionCharacter
                    .Where(text.Contains)
                    .Select(ch => text.IndexOf(ch))
                    .FirstOrDefault(-1);
            }

            return -1;
        }

        /// <summary>
        /// Compass direction characters for latitude
        /// </summary>
        private static readonly char[] LatitudeDirectionCharacter = ['N', 'S'];

        /// <summary>
        /// Compass direction characters for longitude
        /// </summary>
        private static readonly char[] LongitudeDirectionCharacter = ['E', 'W', 'O'];

        /// <summary>
        /// Check if the split latitude and longitude strings actually end with the direction
        /// characters for the other compass direction and swaps them if necessary.
        /// </summary>
        /// <param name="latitude">latitude text to check</param>
        /// <param name="longitude">longitude text to check</param>
        private static void CheckSwapLatLong(ref string latitude, ref string longitude)
        {
            string localLatitude = latitude;
            string localLongitude = longitude;

            bool endsWithSwappedDirectionCharacter =
                Array.Exists(LongitudeDirectionCharacter, localLatitude.EndsWith) &&
                Array.Exists(LatitudeDirectionCharacter, localLongitude.EndsWith);

            bool startsWithSwappedDirectionCharacter =
                Array.Exists(LongitudeDirectionCharacter, localLatitude.StartsWith) &&
                Array.Exists(LatitudeDirectionCharacter, localLongitude.StartsWith);

            if (endsWithSwappedDirectionCharacter ||
                startsWithSwappedDirectionCharacter)
            {
                (longitude, latitude) = (latitude, longitude);
            }

            latitude = latitude.Trim(LatitudeDirectionCharacter).Trim();
            longitude = longitude.Trim(LongitudeDirectionCharacter).Trim();
        }

        /// <summary>
        /// Tries parsing decimal coordinates
        /// </summary>
        /// <param name="latitude">latitude text</param>
        /// <param name="longitude">longitude text</param>
        /// <param name="mapPoint">
        /// map point, containing parsed values of latitude and longitude
        /// </param>
        /// <returns>true when parsing succeeded, or false when not</returns>
        private static bool TryParseDecimals(
            string latitude,
            string longitude,
            out MapPoint? mapPoint)
        {
            mapPoint = null;
            latitude = latitude.TrimEnd('°').TrimEnd();
            longitude = longitude.TrimEnd('°').TrimEnd();

            const NumberStyles numberStyles =
                NumberStyles.AllowDecimalPoint |
                NumberStyles.AllowLeadingSign;

            if (!double.TryParse(
                    latitude,
                    numberStyles,
                    CultureInfo.InvariantCulture,
                    out double parsedLatitude) ||
                !double.TryParse(
                    longitude,
                    numberStyles,
                    CultureInfo.InvariantCulture,
                    out double parsedLongitude))
            {
                return false;
            }

            if (parsedLatitude > 90.0 ||
                parsedLatitude < -90.0)
            {
                return false;
            }

            mapPoint = new MapPoint(
                latitude: parsedLatitude,
                longitude: parsedLongitude);

            return true;
        }

        private static bool TryParseDegreesMinutesSeconds(
            string latitude,
            string longitude,
            out MapPoint? mapPoint)
        {
            if (!TryParseSingleDmsValue(latitude, out double parsedLatitude) ||
                !TryParseSingleDmsValue(longitude, out double parsedLongitude))
            {
                mapPoint = null;
                return false;
            }

            if (parsedLatitude > 90.0 ||
                parsedLatitude < -90.0)
            {
                mapPoint = null;
                return false;
            }

            mapPoint = new MapPoint(
                latitude: parsedLatitude,
                longitude: parsedLongitude);

            return true;
        }

        /// <summary>
        /// Tries to parse a single degrees-minutes-seconds text, either latitude or longitude
        /// </summary>
        /// <param name="latitudeOrLongitudeText">
        /// latitide or longitude text to parse
        /// </param>
        /// <param name="latitudeOrLongitudeValue">
        /// parsed latitude or longitude value
        /// </param>
        /// <returns>true when parsing succeeded, or false when not</returns>
        private static bool TryParseSingleDmsValue(
            string latitudeOrLongitudeText,
            out double latitudeOrLongitudeValue)
        {
            latitudeOrLongitudeValue = 0.0;

            int countDegreeSymbol = latitudeOrLongitudeText.Count(c => c == '°');
            int countMinuteSymbol = latitudeOrLongitudeText.Count(c => c == '\'');

            if (countDegreeSymbol == 1 &&
                !latitudeOrLongitudeText.EndsWith('°') &&
                countMinuteSymbol == 0)
            {
                // has a degree symbol but ends without minute symbol
                latitudeOrLongitudeText += '\'';
                countMinuteSymbol++;
            }

            if (countDegreeSymbol > 1 ||
                countMinuteSymbol > 1 ||
                latitudeOrLongitudeText.Count(c => c == '"') > 1)
            {
                return false;
            }

            if (!latitudeOrLongitudeText.Contains('°') ||
                !latitudeOrLongitudeText.Contains('\'') ||
                latitudeOrLongitudeText.LastIndexOf('°') >= latitudeOrLongitudeText.LastIndexOf('\''))
            {
                return false;
            }

            if (latitudeOrLongitudeText.Contains('\'') &&
                latitudeOrLongitudeText.Contains('"') &&
                latitudeOrLongitudeText.LastIndexOf('"') < latitudeOrLongitudeText.IndexOf('\''))
            {
                return false;
            }

#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            string[] splitValues = latitudeOrLongitudeText.Split('°', '\'');
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"

            // no seconds after minutes?
            if (splitValues.Length == 3 &&
                splitValues[2].Length == 0)
            {
                splitValues = splitValues.Take(2).ToArray();
            }

            if (splitValues.Length < 2 ||
                splitValues.Length > 3 ||
                Array.Exists(splitValues, value => value.Length == 0))
            {
                return false;
            }

            string degreesText = splitValues[0];
            bool isNegative = degreesText.StartsWith('-');

            if (isNegative)
            {
                degreesText = degreesText.TrimStart('-');
            }

            degreesText = degreesText.TrimStart('+');

            if (!double.TryParse(
                degreesText,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out double degreesValue))
            {
                return false;
            }

            latitudeOrLongitudeValue = degreesValue;

            if (!double.TryParse(
                splitValues[1],
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out double minutesValue))
            {
                return false;
            }

            latitudeOrLongitudeValue += minutesValue / 60.0;

            if (splitValues.Length == 3)
            {
                string secondsText = splitValues[2];
                secondsText = secondsText.TrimEnd('"');

                if (!double.TryParse(
                    secondsText,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out double secondsValue))
                {
                    return false;
                }

                latitudeOrLongitudeValue += secondsValue / 3600.0;
            }

            if (isNegative)
            {
                latitudeOrLongitudeValue = -latitudeOrLongitudeValue;
            }

            return true;
        }

        /// <summary>
        /// Tries to parse an URI that may contain location or a placemark
        /// </summary>
        /// <param name="text">URI text</param>
        /// <param name="mapPoint">
        /// map point, containing parsed location coordinates
        /// </param>
        /// <returns>true when parsing succeeded, or false when not</returns>
        private static bool TryParseUri(string text, out MapPoint? mapPoint)
        {
            mapPoint = null;

            if (!Uri.TryCreate(text, UriKind.Absolute, out Uri? uri))
            {
                return false;
            }

            if (TryParseGoogleUri(uri, out mapPoint))
            {
                return true;
            }

            if (TryParseOpenStreetMapUri(uri, out mapPoint))
            {
                return true;
            }

            // parse geo: URI
            if (uri.Scheme.Equals("geo", StringComparison.InvariantCultureIgnoreCase) &&
                !string.IsNullOrEmpty(uri.AbsolutePath) &&
                uri.AbsolutePath != "0,0")
            {
                return TryParse(uri.AbsolutePath, out mapPoint);
            }

            return false;
        }

        /// <summary>
        /// Tries to parse a Google Maps url that contains a "q" query parameter
        /// </summary>
        /// <param name="uri">uri to parse</param>
        /// <param name="mapPoint">
        /// map point, containing parsed location coordinates
        /// </param>
        /// <returns>true when parsing succeeded, or false when not</returns>
        private static bool TryParseGoogleUri(Uri uri, out MapPoint? mapPoint)
        {
            string[] mapsHostnames =
            [
                "maps.google.com",
                "www.google.com",
            ];

            if (!mapsHostnames.Contains(uri.Host.ToLowerInvariant()))
            {
                mapPoint = null;
                return false;
            }

            if (uri.LocalPath.StartsWith(
                "/maps/place/",
                StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = uri.LocalPath.Split(
                    '/',
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    string placeText = parts[2].Replace("+", ",");
                    return TryParse(placeText, out mapPoint);
                }
            }

            Dictionary<string, string> uriParameterMap =
                SplitUriQueryParameter(uri);

            if (!uriParameterMap.TryGetValue("q", out string? param))
            {
                mapPoint = null;
                return false;
            }

            return TryParse(param, out mapPoint);
        }

        /// <summary>
        /// Tries to parse an OpenStreetMap url that contains a "maps" query parameter
        /// </summary>
        /// <param name="uri">uri to parse</param>
        /// <param name="mapPoint">
        /// map point, containing parsed location coordinates
        /// </param>
        /// <returns>true when parsing succeeded, or false when not</returns>
        private static bool TryParseOpenStreetMapUri(Uri uri, out MapPoint? mapPoint)
        {
            if (!uri.Host.Contains(
                "openstreetmap.org",
                StringComparison.InvariantCultureIgnoreCase))
            {
                mapPoint = null;
                return false;
            }

            int mapParamPos = uri.Fragment.IndexOf("map=");

            if (mapParamPos == -1)
            {
                mapPoint = null;
                return false;
            }

            string mapParam = uri.Fragment.Substring(mapParamPos + 4);

            string[] parts = mapParam.Split('/');
            if (parts.Length != 3)
            {
                mapPoint = null;
                return false;
            }

            return TryParseDecimals(
                parts[1],
                parts[2],
                out mapPoint);
        }

        /// <summary>
        /// Splits Uri query parameters into a dictionary
        /// </summary>
        /// <param name="uri">uri to use</param>
        /// <returns>dictionary with all query param keys and values</returns>
        private static Dictionary<string, string> SplitUriQueryParameter(Uri uri)
        {
            return uri.Query.Split('&')
                .Select(parameters => parameters.Split('='))
                .Where(keyAndValue => keyAndValue.Length == 2)
                .ToDictionary(
                    keyAndValue => keyAndValue[0].ToLowerInvariant().Trim('?', '&'),
                    keyAndValue => keyAndValue[1]);
        }
    }
}
