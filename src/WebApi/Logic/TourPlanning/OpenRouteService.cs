using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;

namespace WhereToFly.WebApi.Logic.TourPlanning
{
    /// <summary>
    /// Service to access openrouteservice.org services
    /// </summary>
    public class OpenRouteService
    {
        /// <summary>
        /// Route profile
        /// </summary>
        public enum RouteProfile
        {
            /// <summary>
            /// Route profile "driving-car"
            /// </summary>
            DrivingCar,

            /// <summary>
            /// Route profile "foot-walking"
            /// </summary>
            FootWalking,

            /// <summary>
            /// Route profile "foot-hiking"
            /// </summary>
            FootHiking,
        }

        /// <summary>
        /// REST API address of open route service
        /// </summary>
        private const string OpenRouteServiceApiAddress = "https://api.openrouteservice.org";

        /// <summary>
        /// Mapping of route profiles to text
        /// </summary>
        private static readonly Dictionary<RouteProfile, string> RouteProfileToTextMapping = new()
        {
            { RouteProfile.DrivingCar, "driving-car" },
            { RouteProfile.FootWalking, "foot-walking" },
            { RouteProfile.FootHiking, "foot-hiking" },
        };

        /// <summary>
        /// Route options
        /// </summary>
        public record Options
        {
            /// <summary>
            /// Route profile to use
            /// </summary>
            public RouteProfile Profile { get; set; } = RouteProfile.FootHiking;
        }

        /// <summary>
        /// HTTP client to use
        /// </summary>
        private readonly HttpClient client;

        /// <summary>
        /// Creates a new open route service object
        /// </summary>
        /// <param name="apiKey">API key for the open route service</param>
        public OpenRouteService(string apiKey)
        {
            this.client = new HttpClient
            {
                BaseAddress = new Uri(OpenRouteServiceApiAddress),
            };

            if (!string.IsNullOrEmpty(apiKey))
            {
                this.client.DefaultRequestHeaders.Add("Authorization", apiKey);
            }
        }

        /// <summary>
        /// Calls the "directions" API to plan a tour using given waypoints
        /// </summary>
        /// <param name="waypoints">list of map points to use</param>
        /// <param name="options">route options</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>track object of planned tour</returns>
        /// <exception cref="ArgumentException">
        /// thrown when waypoints list contains less than 2 waypoints
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// thrown when the directions API returns an error message
        /// </exception>
        /// <exception cref="FormatException">
        /// thrown when the directions API returns an unknown JSON format
        /// </exception>
        public async Task<Track> GetDirections(
            IEnumerable<MapPoint> waypoints,
            Options? options = null,
            CancellationToken cancellationToken = default)
        {
            if (waypoints.Count() < 2)
            {
                throw new ArgumentException(
                    "must specify at least two waypoints",
                    nameof(waypoints));
            }

            var localOptions = options ?? new Options();

            string profile = RouteProfileToTextMapping[localOptions.Profile];

            var optionsObject = new
            {
                coordinates =
                    waypoints.Select(
                        point => new double[2]
                        {
                            point.Longitude,
                            point.Latitude,
                        }).ToArray(),
                instructions = false,
            };

            string json = JsonConvert.SerializeObject(optionsObject);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var result = await this.client.PostAsync(
                $"/v2/directions/{profile}/json",
                content,
                cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                var errorDefinition = new { error = string.Empty };

                string errorJson = await result.Content.ReadAsStringAsync();
                var errorObject = JsonConvert.DeserializeAnonymousType(errorJson, errorDefinition);

                throw new InvalidOperationException(
                    $"{(int)result.StatusCode} ({result.ReasonPhrase}): Details: {errorObject?.error}");
            }

            string resultJson = await result.Content.ReadAsStringAsync(cancellationToken);
            var resultObject = JsonConvert.DeserializeObject<ResultObject>(resultJson);

            if (resultObject == null)
            {
                throw new FormatException("directions API returned an unexpected JSON format");
            }

            if (resultObject.Routes == null ||
                resultObject.Routes.Length == 0)
            {
                throw new InvalidOperationException("directions API didn't return any route");
            }

            return new Track(Guid.NewGuid().ToString("B"))
            {
                Name = string.Empty,
                IsFlightTrack = false,
                IsLiveTrack = false,
                LengthInMeter = resultObject.Routes[0].Summary.Distance,
                Duration = TimeSpan.FromSeconds(resultObject.Routes[0].Summary.Duration),
                TrackPoints = DecodeGeometryToTrackPoints(resultObject.Routes[0].Geometry).ToList(),
            };
        }

        /// <summary>
        /// Decodes the geometry passed as polyline string, using
        /// Google's Encoded Polyline format. Adapted from:
        /// https://stackoverflow.com/questions/3852268/c-sharp-implementation-of-googles-encoded-polyline-algorithm
        /// See also: https://developers.google.com/maps/documentation/utilities/polylinealgorithm
        /// </summary>
        /// <param name="polylineString">polyline or geometry string</param>
        /// <returns>list of track points</returns>
        /// <exception cref="ArgumentNullException">
        /// thrown when the polyline string is null or empty
        /// </exception>
        private static IEnumerable<TrackPoint> DecodeGeometryToTrackPoints(string polylineString)
        {
            if (string.IsNullOrEmpty(polylineString))
            {
                throw new ArgumentNullException(nameof(polylineString));
            }

            char[] polylineChars = polylineString.ToCharArray();
            int index = 0;

            double currentLatitude = 0;
            double currentLongitude = 0;

            while (index < polylineChars.Length)
            {
                // next latitude
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

                if (index >= polylineChars.Length)
                {
                    break;
                }

                currentLatitude +=
                    (sum & 1) == 1
                    ? ~(sum >> 1)
                    : (sum >> 1);

                // next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    nextFiveBits = polylineChars[index++] - 63;
                    sum |= (nextFiveBits & 31) << shifter;
                    shifter += 5;
                }
                while (nextFiveBits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && nextFiveBits >= 32)
                {
                    break;
                }

                currentLongitude +=
                    (sum & 1) == 1
                    ? ~(sum >> 1)
                    : (sum >> 1);

                yield return new TrackPoint(
                    Convert.ToDouble(currentLatitude) / 1E5,
                    Convert.ToDouble(currentLongitude) / 1E5,
                    null,
                    null);
            }
        }

        #region Object model for results

        /// <summary>
        /// Result object
        /// </summary>
        internal class ResultObject
        {
            /// <summary>
            /// Bounding box
            /// </summary>
            [JsonProperty("bbox")]
            public double[] BoundingBox { get; set; } = Array.Empty<double>();

            /// <summary>
            /// Routes list
            /// </summary>
            [JsonProperty("routes")]
            public Route[]? Routes { get; set; } = Array.Empty<Route>();

            /// <summary>
            /// Result metadata
            /// </summary>
            [JsonProperty("metadata")]
            public Metadata? Metadata { get; set; }
        }

        /// <summary>
        /// Result metadata
        /// </summary>
        public class Metadata
        {
            /// <summary>
            /// Attribution text
            /// </summary>
            [JsonProperty("attribution")]
            public string Attribution { get; set; } = string.Empty;
        }

        /// <summary>
        /// Route object
        /// </summary>
        public class Route
        {
            /// <summary>
            /// Route summary
            /// </summary>
            [JsonProperty("summary")]
            public Summary Summary { get; set; } = new Summary();

            /// <summary>
            /// Bounding box of route
            /// </summary>
            [JsonProperty("bbox")]
            public double[] BoundingBox { get; set; } = Array.Empty<double>();

            /// <summary>
            /// Geometry polyline string
            /// </summary>
            [JsonProperty("geometry")]
            public string Geometry { get; set; } = string.Empty;

            /// <summary>
            /// Waypoint indices into the geometry
            /// </summary>
            [JsonProperty("way_points")]
            public int[] WaypointIndexList { get; set; } = Array.Empty<int>();
        }

        /// <summary>
        /// Route summary
        /// </summary>
        public class Summary
        {
            /// <summary>
            /// Distance in meter
            /// </summary>
            [JsonProperty("distance")]
            public double Distance { get; set; } = 0.0;

            /// <summary>
            /// Duration in seconds
            /// </summary>
            [JsonProperty("duration")]
            public double Duration { get; set; } = 0.0;
        }
        #endregion
    }
}
