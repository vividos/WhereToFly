using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using WhereToFly.Geo.Model;

[assembly: SuppressMessage(
    "Minor Code Smell",
    "S1075:URIs should not be hardcoded",
    Justification = "The unit test project uses test URLs")]

// Unfortunately we can't run any unit tests in parallel, since some tests
// use the SQLite database, and setting up and tearing down the Maui app
// context also prevents this.
[assembly: DoNotParallelize]

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Helper methods for all unit tests
    /// </summary>
    public static class UnitTestHelper
    {
        /// <summary>
        /// Returns default layer for unit tests
        /// </summary>
        /// <returns>default layer</returns>
        public static Layer GetDefaultLayer()
        {
            return new Layer(Guid.NewGuid().ToString("B"))
            {
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
            return new Location(
                Guid.NewGuid().ToString("B"),
                new MapPoint(47.6764385, 11.8710533, 1685.0))
            {
                Name = "Brecherspitz",
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
            return new Track("track1")
            {
                Name = "Track1",
                IsFlightTrack = false,
                IsLiveTrack = false,
                Color = "FF0000",
                TrackPoints =
                [
                    new TrackPoint(47.754076, 12.352277, 1234.0, null)
                    {
                        Time = DateTime.Today + TimeSpan.FromHours(1.0),
                    },
                    new TrackPoint(46.017779, 11.900711, 778.2, null)
                    {
                        Time = DateTime.Today + TimeSpan.FromHours(2.0),
                    },
                ],
            };
        }
    }
}
