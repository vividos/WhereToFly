using Refit;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.WebApi.Logic.Services.FindMeSpot;

namespace WhereToFly.WebApi.Logic.Services
{
    /// <summary>
    /// Data service for the Find Me SPOT REST web service
    /// </summary>
    public class FindMeSpotTrackerDataService
    {
        /// <summary>
        /// Base URL for the Find Me SPOT REST web service
        /// </summary>
        private const string BaseUrl = "https://api.findmespot.com/spot-main-web/consumer/rest-api/2.0";

        /// <summary>
        /// Access to FindMeSpot REST API
        /// </summary>
        private readonly IFindMeSpotService findMeSpotApi;

        /// <summary>
        /// Creates a new data service object
        /// </summary>
        public FindMeSpotTrackerDataService()
        {
            this.findMeSpotApi = RestService.For<IFindMeSpotService>(BaseUrl);
        }

        /// <summary>
        /// Returns data for live waypoint with given ID
        /// </summary>
        /// <param name="id">live waypoint ID</param>
        /// <returns>live waypoint data</returns>
        public async Task<LiveWaypointData> GetDataAsync(string id)
        {
            var liveWaypointId = LiveWaypointID.FromString(id);
            Debug.Assert(
                liveWaypointId.Type == LiveWaypointID.WaypointType.FindMeSpot,
                "live waypoint ID must be of FindMeSpot type!");

            string liveFeedId = liveWaypointId.Data;
            var result = await this.findMeSpotApi.GetLatest(liveFeedId);

            var latestMessage = result.Response.FeedMessageResponse.Messages.Message;

            return new LiveWaypointData
            {
                ID = id,
                TimeStamp = new DateTimeOffset(latestMessage.DateTime),
                Latitude = latestMessage.Latitude,
                Longitude = latestMessage.Longitude,
                Altitude = latestMessage.Altitude,
                Name = "FindMeSpot " + result.Response.FeedMessageResponse.Feed.Name,
                Description = FormatDescription(
                    result.Response.FeedMessageResponse.Feed,
                    latestMessage),
                DetailsLink = FormatSpotLink(liveFeedId),
            };
        }

        /// <summary>
        /// Formats a description based on feed and latest message
        /// </summary>
        /// <param name="feed">feed object</param>
        /// <param name="latestMessage">latest message</param>
        /// <returns>markdown-formatted description of feed and latest message</returns>
        private static string FormatDescription(Model.Feed feed, Model.Message latestMessage)
        {
            return $"FindMeSpot feed for {feed.Name}\n" +
                $"Current coordinates: Latitude: {latestMessage.Latitude}\n" +
                $"Longitude: {latestMessage.Longitude}\n" +
                $"Altitude: {latestMessage.Altitude}\n" +
                $"Date/time: {latestMessage.DateTime}\n";
        }

        /// <summary>
        /// Formats a link to the Find Me SPOT webpage
        /// </summary>
        /// <param name="liveFeedId">live feed ID to use</param>
        /// <returns>web link URL</returns>
        private static string FormatSpotLink(string liveFeedId)
        {
            return $"https://share.findmespot.com/shared/faces/viewspots.jsp?glId={liveFeedId}";
        }
    }
}
