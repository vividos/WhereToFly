using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.Geo;
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
            /// Route profile "driving-hgv"
            /// </summary>
            DrivingHeavyGoodsVehicle,

            /// <summary>
            /// Route profile "foot-walking"
            /// </summary>
            FootWalking,

            /// <summary>
            /// Route profile "foot-hiking"
            /// </summary>
            FootHiking,

            /// <summary>
            /// Route profile "cycling-regular"
            /// </summary>
            CyclingRegular,

            /// <summary>
            /// Route profile "cycling-road"
            /// </summary>
            CyclingRoad,

            /// <summary>
            /// Route profile "cycling-mountain"
            /// </summary>
            CyclingMountain,

            /// <summary>
            /// Route profile "cycling-electric"
            /// </summary>
            CyclingElectric,
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
            { RouteProfile.DrivingHeavyGoodsVehicle, "driving-hgv" },
            { RouteProfile.FootWalking, "foot-walking" },
            { RouteProfile.FootHiking, "foot-hiking" },
            { RouteProfile.CyclingRegular, "cycling-regular" },
            { RouteProfile.CyclingRoad, "cycling-road" },
            { RouteProfile.CyclingMountain, "cycling-mountain" },
            { RouteProfile.CyclingElectric, "cycling-electric" },
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
                elevation = true,
            };

            string json = JsonConvert.SerializeObject(optionsObject);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var result = await this.client.PostAsync(
                $"/v2/directions/{profile}/json",
                content,
                cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                string errorJson = await result.Content.ReadAsStringAsync();
                var errorObject = JsonConvert.DeserializeObject<ErrorInfo>(errorJson);

                throw new InvalidOperationException(
                    $"{(int)result.StatusCode} ({result.ReasonPhrase}): Details: {errorObject?.Error?.Message}");
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

            var trackPoints = EncodedPolylineGeometry.DecodeGeometryToTrackPoints(
                resultObject.Routes[0].Geometry,
                optionsObject.elevation);

            return new Track(Guid.NewGuid().ToString("B"))
            {
                Name = string.Empty,
                IsFlightTrack = false,
                IsLiveTrack = false,
                LengthInMeter = resultObject.Routes[0].Summary.Distance,
                Duration = TimeSpan.FromSeconds(resultObject.Routes[0].Summary.Duration),
                TrackPoints = trackPoints.ToList(),
            };
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

        /// <summary>
        /// Error info
        /// </summary>
        public class ErrorInfo
        {
            /// <summary>
            /// Error details
            /// </summary>
            [JsonProperty("error")]
            public ErrorDetails? Error { get; set; }

            /// <summary>
            /// Error details
            /// </summary>
            public class ErrorDetails
            {
                /// <summary>
                /// Error code
                /// </summary>
                [JsonProperty("code")]
                public int Code { get; set; }

                /// <summary>
                /// Error message
                /// </summary>
                [JsonProperty("message")]
                public string? Message { get; set; }
            }
        }
        #endregion
    }
}
