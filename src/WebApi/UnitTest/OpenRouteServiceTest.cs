using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;
using WhereToFly.WebApi.Logic.TourPlanning;

namespace WhereToFly.WebApi.UnitTest
{
    /// <summary>
    /// Tests for <see cref="OpenRouteService"/> class
    /// </summary>
    [TestClass]
    public class OpenRouteServiceTest
    {
        /// <summary>
        /// Tests getting directions for hiking
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        [Ignore("test can only be run manually, since it uses a live service")]
        public async Task TestGetDirections_Hiking()
        {
            // set up
            string apiKey = Environment.GetEnvironmentVariable("OPENROUTESERVICE_API_KEY")
                ?? throw new InvalidOperationException("couldn't find environment variable OPENROUTESERVICE_API_KEY");

            var service = new OpenRouteService(apiKey);

            // run
            Track track = await service.GetDirections(
                [
                    new MapPoint(47.70599, 11.87451, 809.0), // Bahnhof Neuhaus
                    new MapPoint(47.6764385, 11.8710533, 1685.0), // Brecherspitz
                ],
                new OpenRouteService.Options
                {
                    Profile = OpenRouteService.RouteProfile.FootHiking,
                });

            // check
            Assert.IsGreaterThan(0, track.Id.Length, "ID must be set");
            Assert.IsGreaterThan(0, track.Attribution?.Length ?? -1, "attribution must be set");
            Assert.IsGreaterThan(0, track.Duration.TotalMinutes, "duration must be set");
            Assert.IsGreaterThan(0, track.LengthInMeter, "length must be set");
            Assert.IsFalse(track.IsFlightTrack, "track must not be a flight");
            Assert.IsNotEmpty(track.TrackPoints, "track point list must be filled");
        }
    }
}
