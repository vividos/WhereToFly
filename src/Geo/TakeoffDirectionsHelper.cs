using System;
using System.Collections.Generic;
using System.Linq;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo
{
    /// <summary>
    /// Helper methods for the TakeoffDirections enum
    /// </summary>
    public static class TakeoffDirectionsHelper
    {
        /// <summary>
        /// Tries to parse a text string for takeoff directions. When the text contains
        /// parenthesis, only the text in the parenthesis is parsed. The takeoff directions can
        /// be single directions, separated by comma, or direction ranges, separated by a dash,
        /// or a combination of both.
        /// </summary>
        /// <param name="text">text to parse</param>
        /// <param name="takeoffDirections">flag enum that contains all directions</param>
        /// <returns>true when parsing was successful, false when not</returns>
        public static bool TryParse(string text, out TakeoffDirections takeoffDirections)
        {
            takeoffDirections = TakeoffDirections.None;

            int startBracket = text.IndexOf("(");
            if (startBracket != -1)
            {
                int endBracket = text.IndexOf(")", startBracket + 1);
                if (endBracket != -1)
                {
                    return TryParse(
                        text.Substring(startBracket + 1, endBracket - startBracket - 1),
                        out takeoffDirections);
                }
            }

            text = text.ToUpperInvariant().Replace(" ", string.Empty);

            if (text.Any((ch) => !IsValidTakeoffChar(ch)))
            {
                return false;
            }

            text = text.Replace("O", "E"); // replace german east letter

            foreach (var range in text.Split(','))
            {
                if (!TryParseDirectionOrRange(range, ref takeoffDirections))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns if given character is a valid takeoff directions character
        /// </summary>
        /// <param name="ch">character to check</param>
        /// <returns>true when valid character, false when not</returns>
        private static bool IsValidTakeoffChar(char ch)
        {
            return "NSEWO-,".Contains(ch);
        }

        /// <summary>
        /// Mapping between takeoff direction string to takeoff direction enum value
        /// </summary>
        private static readonly Dictionary<string, TakeoffDirections> MapDirectionToEnum =
            new()
            {
                { "N", TakeoffDirections.N },
                { "NNE", TakeoffDirections.NNE },
                { "NE", TakeoffDirections.NE },
                { "ENE", TakeoffDirections.ENE },
                { "E", TakeoffDirections.E },
                { "ESE", TakeoffDirections.ESE },
                { "SE", TakeoffDirections.SE },
                { "SSE", TakeoffDirections.SSE },
                { "S", TakeoffDirections.S },
                { "SSW", TakeoffDirections.SSW },
                { "SW", TakeoffDirections.SW },
                { "WSW", TakeoffDirections.WSW },
                { "W", TakeoffDirections.W },
                { "WNW", TakeoffDirections.WNW },
                { "NW", TakeoffDirections.NW },
                { "NNW", TakeoffDirections.NNW },
            };

        /// <summary>
        /// Tries to parse a direction or range of directions text
        /// </summary>
        /// <param name="dirOrRange">direction or range of directions</param>
        /// <param name="takeoffDirections">flag enum that contains all directions</param>
        /// <returns>true when parsing was successful, false when not</returns>
        private static bool TryParseDirectionOrRange(string dirOrRange, ref TakeoffDirections takeoffDirections)
        {
            int dashIndex = dirOrRange.IndexOf("-");
            if (dashIndex == -1)
            {
                if (!MapDirectionToEnum.ContainsKey(dirOrRange))
                {
                    return false;
                }

                takeoffDirections |= MapDirectionToEnum[dirOrRange];
                return true;
            }

            return TryParseRange(dirOrRange, ref takeoffDirections);
        }

        /// <summary>
        /// Tries to parse a range of directions text
        /// </summary>
        /// <param name="range">range of directions</param>
        /// <param name="takeoffDirections">flag enum that contains all directions</param>
        /// <returns>true when parsing was successful, false when not</returns>
        private static bool TryParseRange(string range, ref TakeoffDirections takeoffDirections)
        {
            var parts = range.Split('-');
            if (parts.Length != 2)
            {
                return false;
            }

            var startDirectionText = parts[0];
            var endDirectionText = parts[1];

            if (!MapDirectionToEnum.ContainsKey(startDirectionText) ||
                !MapDirectionToEnum.ContainsKey(endDirectionText))
            {
                return false;
            }

            var startDirection = MapDirectionToEnum[startDirectionText];
            var endDirection = MapDirectionToEnum[endDirectionText];

            // determines which way around the compass rose
            int startBitPos = (int)Math.Log((int)startDirection, 2.0);
            int endBitPos = (int)Math.Log((int)endDirection, 2.0);

            if (endBitPos < startBitPos)
            {
                endBitPos += 16;
            }

            bool directionClockwise = Math.Abs(startBitPos - endBitPos) < 8;

            for (TakeoffDirections currentDirection = startDirection; currentDirection != endDirection;)
            {
                takeoffDirections |= currentDirection;

                // advance direction
                currentDirection = directionClockwise ? GetNextDirection(currentDirection) : GetPreviousDirection(currentDirection);
            }

            takeoffDirections |= endDirection;

            return true;
        }

        /// <summary>
        /// Returns the next direction in clockwise direction; wraps around when at the last
        /// direction
        /// </summary>
        /// <param name="direction">direction to use</param>
        /// <returns>next direction value</returns>
        private static TakeoffDirections GetNextDirection(TakeoffDirections direction)
        {
            return direction == TakeoffDirections.Last
                ? TakeoffDirections.First
                : (TakeoffDirections)((int)direction << 1);
        }

        /// <summary>
        /// Returns the previous direction, or next in counter-clockwise direction; wraps around
        /// when at the first direction
        /// </summary>
        /// <param name="direction">direction to use</param>
        /// <returns>previous direction value</returns>
        private static TakeoffDirections GetPreviousDirection(TakeoffDirections direction)
        {
            return direction == TakeoffDirections.First
                ? TakeoffDirections.Last
                : (TakeoffDirections)((int)direction >> 1);
        }

        /// <summary>
        /// Modifies takeoff directions from the TakeoffDirectionsView that only has 8 cardinal
        /// directions, to a takeoff directions value that can be used for filtering location
        /// entries. This happens by merging the three-letter directions (e.g. NNE) to their
        /// adjacent two- or one-letter directions.
        /// </summary>
        /// <param name="directions">directions to modify</param>
        /// <returns>modified directions</returns>
        public static TakeoffDirections ModifyAdjacentDirectionsFromView(TakeoffDirections directions)
        {
            TakeoffDirections directionToCheck = TakeoffDirections.First;

            for (int counter = 0; counter < 8; counter++)
            {
                if (directions.HasFlag(directionToCheck))
                {
                    directions |= GetNextDirection(directionToCheck);
                    directions |= GetPreviousDirection(directionToCheck);
                }

                directionToCheck = GetNextDirection(GetNextDirection(directionToCheck));
            }

            return directions;
        }
    }
}
