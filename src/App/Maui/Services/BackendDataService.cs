using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;
using WhereToFly.Shared.Model.Serializers;

namespace WhereToFly.App.Services
{
    /// <summary>
    /// Data service that communicates with the backend data service (the project
    /// WhereToFly.WebApi.Core).
    /// </summary>
    public class BackendDataService
    {
        /// <summary>
        /// Base URL for the WebApi REST web service
        /// </summary>
#pragma warning disable S1075 // URIs should not be hardcoded
        private const string BaseUrl = "https://wheretoflywebapi.azurewebsites.net";
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// HTTP client
        /// </summary>
        private readonly HttpClient client;

        /// <summary>
        /// Creates a new backend data service object
        /// </summary>
        public BackendDataService()
        {
            this.client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
            };
        }

        /// <summary>
        /// Returns the current app configuration to be used.
        /// </summary>
        /// <param name="appVersion">app version requesting the app configuration</param>
        /// <returns>app configuration</returns>
        public async Task<AppConfig> GetAppConfigAsync(string appVersion)
        {
            return await this.Get<AppConfig>(
                $"/api/AppConfig?appVersion={appVersion}");
        }

        /// <summary>
        /// Returns a favicon URL representing the icon for a given website.
        /// </summary>
        /// <param name="websiteUrl">website to get favicon URL</param>
        /// <returns>favicon URL</returns>
        public async Task<string> GetFaviconUrlAsync(string websiteUrl)
        {
            Debug.WriteLine($"Backend: Retrieving favicon for URL {websiteUrl}");

            websiteUrl = System.Net.WebUtility.UrlEncode(websiteUrl);

            return await this.client.GetStringAsync(
                $"/api/FaviconUrl?websiteUrl={websiteUrl}");
        }

        /// <summary>
        /// Retrieves latest info about a live waypoint, including new coordinates and
        /// description.
        /// </summary>
        /// <param name="liveWaypointId">live waypoint ID</param>
        /// <returns>query result for live waypoint</returns>
        public async Task<LiveWaypointQueryResult> GetLiveWaypointDataAsync(string liveWaypointId)
        {
            LiveWaypointQueryResult? result =
                await this.Get<LiveWaypointQueryResult>(
                    $"/api/LiveWaypoint?id={liveWaypointId}");

            if (result.Data != null)
            {
                result.Data.ID = System.Net.WebUtility.UrlDecode(result.Data.ID);
            }

            return result;
        }

        /// <summary>
        /// Retrieves latest info about a live track, including new list of track points and
        /// description.
        /// </summary>
        /// <param name="liveTrackId">live track ID</param>
        /// <param name="lastTrackPointTime">
        /// last track point that the client already has received, or null when no track points
        /// are known yet
        /// </param>
        /// <returns>query result for live track</returns>
        public async Task<LiveTrackQueryResult> GetLiveTrackDataAsync(
            string liveTrackId,
            DateTimeOffset? lastTrackPointTime)
        {
            LiveTrackQueryResult result =
                await this.Get<LiveTrackQueryResult>(
                    $"/api/LiveTrack?id={liveTrackId}&time={lastTrackPointTime:o}");

            result.Data.ID = System.Net.WebUtility.UrlDecode(result.Data.ID);

            return result;
        }

        /// <summary>
        /// Plans a tour with given parameters and returns a planned tour, including description
        /// and a track.
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        /// <returns>planned tour</returns>
        public async Task<PlannedTour> PlanTourAsync(PlanTourParameters planTourParameters)
        {
            return await this.Post<PlannedTour, PlanTourParameters>(
                "/api/PlanTour",
                planTourParameters);
        }

        /// <summary>
        /// Returns a list of nearby POIs in the map rectangle with the same integer latitude
        /// and longitude values.
        /// </summary>
        /// <param name="latitude">integer latitude value</param>
        /// <param name="longitude">integer longitude value</param>
        /// <returns>list of locations</returns>
        public async Task<IEnumerable<Location>> FindNearbyPoisAsync(int latitude, int longitude)
        {
            return await this.Get<IEnumerable<Location>>(
                $"/api/NearbyPois?latitude={latitude}&longitude={longitude}");
        }

        /// <summary>
        /// Sends a HTTP GET request to the backend and deserializes the JSON result
        /// </summary>
        /// <typeparam name="TResult">type of result to deserialize to</typeparam>
        /// <param name="requestUri">request URI</param>
        /// <returns>deserialized result object</returns>
        /// <exception cref="Exception">
        /// thrown when the network request or deserialization of result failed
        /// </exception>
        private async Task<TResult> Get<TResult>(string requestUri)
            where TResult : class
        {
            var resultJsonTypeInfo =
                SharedModelJsonSerializerContext.Default.GetTypeInfo(typeof(TResult));

            if (resultJsonTypeInfo == null)
            {
                throw new InvalidOperationException(
                    $"JsonSerializerContext has no serializer for result type {typeof(TResult).FullName} included!");
            }

            using var stream = await this.client.GetStreamAsync(requestUri);

            object? result = await JsonSerializer.DeserializeAsync(
                stream,
                resultJsonTypeInfo);

            return result as TResult
                ?? throw new InvalidOperationException("couldn't deserialize the GET result stream");
        }

        /// <summary>
        /// Sends a HTTP POST request to the backend, including JSON parameter in the body and
        /// deserializes the JSON result
        /// </summary>
        /// <typeparam name="TResult">type of result to deserialize to</typeparam>
        /// <typeparam name="TParam">type of the parameter to serialize</typeparam>
        /// <param name="requestUri">request URI</param>
        /// <param name="postParam">POST parameter sent in the body</param>
        /// <returns>deserialized result object</returns>
        /// <exception cref="Exception">
        /// thrown when the network request or deserialization of result failed
        /// </exception>
        private async Task<TResult> Post<TResult, TParam>(
            string requestUri,
            TParam postParam)
            where TResult : class
        {
            var paramJsonTypeInfo =
                SharedModelJsonSerializerContext.Default.GetTypeInfo(typeof(TParam));

            if (paramJsonTypeInfo == null)
            {
                throw new InvalidOperationException(
                    $"JsonSerializerContext has no serializer for parameter type {typeof(TParam).FullName} included!");
            }

            var resultJsonTypeInfo =
                SharedModelJsonSerializerContext.Default.GetTypeInfo(typeof(TResult));

            if (resultJsonTypeInfo == null)
            {
                throw new InvalidOperationException(
                    $"JsonSerializerContext has no serializer for result type {typeof(TResult).FullName} included!");
            }

            var content = JsonContent.Create(postParam, paramJsonTypeInfo);

            using var responseMessage = await this.client.PostAsync(
                requestUri,
                content);

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    responseMessage.ReasonPhrase);
            }

            using var stream = await responseMessage.Content.ReadAsStreamAsync();

            object? result = await JsonSerializer.DeserializeAsync(
                stream,
                resultJsonTypeInfo);

            return result as TResult
                ?? throw new InvalidOperationException("couldn't deserialize the POST result stream");
        }
    }
}
