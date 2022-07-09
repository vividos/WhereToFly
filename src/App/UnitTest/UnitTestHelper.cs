using System;
using System.Collections.Generic;
using System.IO;
using WhereToFly.Geo.Model;

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
        public static string TestAssetsPath =>
            Path.Combine(
                Path.GetDirectoryName(typeof(UnitTestHelper).Assembly.Location),
                "Assets");

        /// <summary>
        /// Returns default layer for unit tests
        /// </summary>
        /// <returns>default layer</returns>
        public static Layer GetDefaultLayer()
        {
            return new Layer
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = "DefaultLayer",
                Description = "Default description",
                LayerType = LayerType.CzmlLayer,
                IsVisible = true,
                Data = "abc123xyz",
            };
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
                InternetLink = "https://de.wikipedia.org/wiki/Brecherspitz",
            };
        }

        /// <summary>
        /// Returns default track for unit tests
        /// </summary>
        /// <returns>default track</returns>
        public static Track GetDefaultTrack()
        {
            return new Track
            {
                Id = "track1",
                Name = "Track1",
                IsFlightTrack = false,
                IsLiveTrack = false,
                Color = "FF0000",
                TrackPoints = new List<TrackPoint>
                {
                    new TrackPoint(47.754076, 12.352277, 1234.0, null)
                    {
                        Time = DateTime.Today + TimeSpan.FromHours(1.0),
                    },
                    new TrackPoint(46.017779, 11.900711, 778.2, null)
                    {
                        Time = DateTime.Today + TimeSpan.FromHours(2.0),
                    },
                },
            };
        }
    }
}
