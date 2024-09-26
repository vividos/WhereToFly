using System;
using System.Collections.Generic;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo
{
    /// <summary>
    /// Helper class for encoded polyline geometry:
    /// https://developers.google.com/maps/documentation/utilities/polylinealgorithm
    /// </summary>
    public static class EncodedPolylineGeometry
    {
        /// <summary>
        /// Decodes the geometry passed as polyline string, using
        /// Google's Encoded Polyline format. Adapted from:
        /// https://stackoverflow.com/questions/3852268/c-sharp-implementation-of-googles-encoded-polyline-algorithm
        /// </summary>
        /// <param name="polylineString">polyline or geometry string</param>
        /// <param name="withElevation">
        /// when true, the geometry string also contains elevations as third value
        /// </param>
        /// <returns>list of track points</returns>
        /// <exception cref="ArgumentNullException">
        /// thrown when the polyline string is null or empty
        /// </exception>
        public static IEnumerable<TrackPoint> DecodeGeometryToTrackPoints(
            string polylineString,
            bool withElevation)
        {
            if (string.IsNullOrEmpty(polylineString))
            {
                throw new ArgumentNullException(nameof(polylineString));
            }

            char[] polylineChars = polylineString.ToCharArray();
            int index = 0;

            double currentLatitude = 0;
            double currentLongitude = 0;
            double? currentAltitude = null;

            while (index < polylineChars.Length)
            {
                // next latitude
                if (!TryGetNextValue(polylineChars, ref index, out double value))
                {
                    break;
                }

                currentLatitude += value;

                // next longitude
                if (!TryGetNextValue(polylineChars, ref index, out value))
                {
                    break;
                }

                currentLongitude += value;

                if (withElevation)
                {
                    if (!TryGetNextValue(polylineChars, ref index, out value))
                    {
                        break;
                    }

                    if (currentAltitude.HasValue)
                    {
                        currentAltitude += value;
                    }
                    else
                    {
                        currentAltitude = value;
                    }
                }

                yield return new TrackPoint(
                    Convert.ToDouble(currentLatitude) / 1E5,
                    Convert.ToDouble(currentLongitude) / 1E5,
                    !currentAltitude.HasValue ? null : Convert.ToDouble(currentAltitude.Value) / 1E2,
                    null);
            }
        }

        /// <summary>
        /// Tries to get the next 5-bit value from the polyline character array
        /// </summary>
        /// <param name="polylineChars">polyline character array</param>
        /// <param name="index">current index</param>
        /// <param name="value">next value</param>
        /// <returns>true when successful, or false when at the end</returns>
        private static bool TryGetNextValue(char[] polylineChars, ref int index, out double value)
        {
            value = 0.0;

            int sum = 0;
            int shifter = 0;
            int nextFiveBits;

            do
            {
                nextFiveBits = polylineChars[index++] - 63;
                sum |= (nextFiveBits & 31) << shifter;
                shifter += 5;
            }
            while (nextFiveBits >= 32 && index < polylineChars.Length);

            if (index >= polylineChars.Length && nextFiveBits >= 32)
            {
                return false;
            }

            value = (sum & 1) == 1
                ? ~(sum >> 1)
                : (sum >> 1);

            return true;
        }
    }
}
