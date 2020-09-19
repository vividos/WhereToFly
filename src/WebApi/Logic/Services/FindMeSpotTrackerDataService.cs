using Refit;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WhereToFly.Shared.Model;
using WhereToFly.WebApi.Logic.Services.FindMeSpot;

[assembly: InternalsVisibleTo("WhereToFly.WebApi.UnitTest")]

namespace WhereToFly.WebApi.Logic.Services
{
    /// <summary>
    /// Data service for the Find Me SPOT REST web service
    /// </summary>
    internal class FindMeSpotTrackerDataService
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        /// <summary>
        /// Base URL for the Find Me SPOT REST web service
        /// </summary>
        private const string BaseUrl = "https://api.findmespot.com/spot-main-web/consumer/rest-api/2.0";
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// Access to FindMeSpot REST API
        /// </summary>
        private readonly IFindMeSpotService findMeSpotApi;

        /// <summary>
        /// Date/time of last request, or null when no request was made yet
        /// </summary>
        private DateTimeOffset? lastRequest = null;

        /// <summary>
        /// Creates a new data service object
        /// </summary>
        public FindMeSpotTrackerDataService()
        {
            this.findMeSpotApi = RestService.For<IFindMeSpotService>(BaseUrl);
        }

        /// <summary>
        /// Returns next possible request date for given MapShare identifier
        /// </summary>
        /// <param name="uri">live waypoint ID</param>
        /// <returns>next possible request date</returns>
        public DateTimeOffset GetNextRequestDate(AppResourceUri uri)
        {
            return this.lastRequest.HasValue ? this.lastRequest.Value + TimeSpan.FromMinutes(1.0) : DateTimeOffset.Now;
        }

        /// <summary>
        /// Returns data for live waypoint with given app resource URI
        /// </summary>
        /// <param name="uri">live waypoint ID</param>
        /// <returns>live waypoint data</returns>
        public async Task<LiveWaypointData> GetDataAsync(AppResourceUri uri)
        {
            Debug.Assert(
                uri.Type == AppResourceUri.ResourceType.FindMeSpotPos,
                "app resource URI must be of FindMeSpotPos type!");

            string liveFeedId = uri.Data;

            Model.RootObject result;
            if (liveFeedId != "xxx")
            {
                result = await this.findMeSpotApi.GetLatest(liveFeedId);

                this.lastRequest = DateTimeOffset.Now;
            }
            else
            {
                // support for unit test
                string feedText = @"{""response"":{""feedMessageResponse"":{""count"":1,""feed"":{""id"":""abc123"",""name"":""name1"",""description"":""desc1"",""status"":""ACTIVE"",""usage"":0,""daysRange"":7,""detailedMessageShown"":true,""type"":""SHARED_PAGE""},""totalCount"":1,""activityCount"":0,""messages"":{""message"":{""@clientUnixTime"":""0"",""id"":942821234,""messengerId"":""0-123456"",""messengerName"":""Spot"",""unixTime"":1521991234,""messageType"":""UNLIMITED-TRACK"",""latitude"":48.12345,""longitude"":11.12345,""modelId"":""SPOT3"",""showCustomMsg"":""Y"",""dateTime"":""2018-03-26T20:52:00+0000"",""batteryState"":""GOOD"",""hidden"":0,""altitude"":1234}}}}}";
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.RootObject>(feedText);
            }

            var latestMessage = result.Response.FeedMessageResponse.Messages.Message;

            return new LiveWaypointData
            {
                ID = uri.ToString(),
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
