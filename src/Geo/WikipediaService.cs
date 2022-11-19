using System.Collections.Generic;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo
{
    /// <summary>
    /// Wikipedia service class
    /// </summary>
    public static class WikipediaService
    {
        /// <summary>
        /// Mapping from wikipedia tags text to location type
        /// </summary>
        private static readonly Dictionary<string, LocationType>
            WikipediaTagsToLocationTypeMapping = new()
        {
            { "natural=peak", LocationType.Summit },
            { "natural=saddle", LocationType.Pass },
        };

        /// <summary>
        /// Tries to map wikipedia tags, found in the text, such as natural=*, to location types,
        /// if possible.
        /// </summary>
        /// <param name="text">text to check</param>
        /// <param name="wikipediaLocationType">location type</param>
        /// <returns>true when successful, or false when not</returns>
        public static bool TryMapWikipediaTagsToLocationType(
            string text,
            out LocationType wikipediaLocationType)
        {
            wikipediaLocationType = LocationType.Waypoint;

            foreach (string wikipediaTag in WikipediaTagsToLocationTypeMapping.Keys)
            {
                if (text.Contains(wikipediaTag))
                {
                    wikipediaLocationType =
                        WikipediaTagsToLocationTypeMapping[wikipediaTag];
                    return true;
                }
            }

            return false;
        }
    }
}
