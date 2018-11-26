using System;
using System.IO;
using WhereToFly.App.Model;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Helper methods for all unit tests
    /// </summary>
    public static class UnitTestHelper
    {
        /// <summary>
        /// Returns the Assets path for all unit tests; place your test files in the Assets folder
        /// and mark them with "Content" and "Copy if newer".
        /// </summary>
        public static string TestAssetsPath
        {
            get
            {
                return Path.Combine(
                    Path.GetDirectoryName(typeof(UnitTestHelper).Assembly.Location),
                    "Assets");
            }
        }

        /// <summary>
        /// Returns default location for unit tests
        /// </summary>
        /// <returns>default location</returns>
        public static Location GetDefaultLocation()
        {
            return new Location
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = "Brecherspitz",
                MapLocation = new MapPoint(47.6764385, 11.8710533, 1685.0),
                Description = "Herrliche Aussicht über die drei Seen Schliersee im Norden, Tegernsee im Westen und den Spitzingsee im Süden.",
                Type = LocationType.Summit,
                InternetLink = "https://de.wikipedia.org/wiki/Brecherspitz"
            };
        }
    }
}
