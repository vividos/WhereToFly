using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;
using System.Text.Json;
using WhereToFly.Shared.Model;
using WhereToFly.Shared.Model.Serializers;

namespace WhereToFly.Web.LiveTracking.Pages
{
    /// <summary>
    /// Page model for index page
    /// </summary>
    public class IndexModel : PageModel
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        /// <summary>
        /// Base URL for the WebApi REST web service
        /// </summary>
        private const string BaseUrl = "https://wheretoflywebapi.azurewebsites.net";
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// Backend Web API access
        /// </summary>
        private readonly IBackendWebApi backendWebApi;

        /// <summary>
        /// Data sent by UpdateLiveWaypoint GET request from page
        /// </summary>
        /// <param name="Uri">the URI of the live waypoint to update</param>
        public record UpdateLiveWaypointData(string Uri);

        /// <summary>
        /// Data sent by UpdateLiveTrack GET request from page
        /// </summary>
        /// <param name="Uri">the URI of the live track to update</param>
        /// <param name="LastTrackPointTime">last track point time; may be null</param>
        public record UpdateLiveTrackData(string Uri, DateTimeOffset? LastTrackPointTime);

        /// <summary>
        /// Infos about a single live tracking object
        /// </summary>
        /// <param name="Name">name of live tracking waypoint</param>
        /// <param name="Uri">live tracking waypoint URI</param>
        /// <param name="IsLiveTrack">indicates if it's a live track or a live waypoint</param>
        /// <param name="IsFlightTrack">indicates if it's a flight position/track or ground-based tracking</param>
        public record LiveTrackingInfo(string Name, string Uri, bool IsLiveTrack, bool IsFlightTrack = true);

        /// <summary>
        /// List of live tracking infos; used as the view data of the page
        /// </summary>
        [ViewData]
        public List<LiveTrackingInfo> LiveTrackingInfoList { get; private set; }

        /// <summary>
        /// Creates a new index model
        /// </summary>
        public IndexModel()
        {
            this.backendWebApi =
                RestService.For<IBackendWebApi>(
                    BaseUrl,
                    new RefitSettings
                    {
                        ContentSerializer = new SystemTextJsonContentSerializer(
                            new JsonSerializerOptions
                            {
                                TypeInfoResolver = SharedModelJsonSerializerContext.Default,
                            }),
                    });

#pragma warning disable S1075 // URIs should not be hardcoded
            const string TestPosDataUri = "where-to-fly://TestPos/data";
#pragma warning restore S1075 // URIs should not be hardcoded

            this.LiveTrackingInfoList =
            [
                new LiveTrackingInfo(
                    "TestPos Schliersee",
                    TestPosDataUri,
                    default)
            ];
        }

        /// <summary>
        /// GET handler when page parameters uri and name are passed to set a custom live waypoint
        /// to show in the page.
        /// </summary>
        /// <param name="uri">live waypoint uri to show</param>
        /// <param name="name">name of live waypoint to show</param>
        /// <param name="isFlightTrack">indicates if the live track</param>
        public void OnGet(string uri, string name, bool isFlightTrack = true)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return;
            }

            var liveWaypointUri = new AppResourceUri(uri);

            if (liveWaypointUri.IsValid)
            {
                this.LiveTrackingInfoList =
                [
                    new LiveTrackingInfo(
                        name ?? "Live Waypoint",
                        liveWaypointUri.ToString(),
                        liveWaypointUri.IsTrackResourceType,
                        isFlightTrack)
                ];
            }
        }

        /// <summary>
        /// Called when page requests an update for a live waypoint
        /// </summary>
        /// <param name="data">update live waypoint data</param>
        /// <returns>
        /// JSON result of query, either a LiveWaypointQueryResult or an exception text
        /// </returns>
        public async Task<JsonResult> OnPostUpdateLiveWaypointAsync(
            [FromBody] UpdateLiveWaypointData data)
        {
            string liveWaypointId = data.Uri;

            try
            {
                LiveWaypointQueryResult queryResult =
                    await this.backendWebApi.GetLiveWaypointDataAsync(liveWaypointId);

                return new JsonResult(queryResult);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }

        /// <summary>
        /// Called when page requests an update for a live track
        /// </summary>
        /// <param name="data">update live track data</param>
        /// <returns>
        /// JSON result of query, either a LiveTrackQueryResult or an exception text
        /// </returns>
        public async Task<JsonResult> OnPostUpdateLiveTrackAsync(
            [FromBody] UpdateLiveTrackData data)
        {
            string liveTrackId = data.Uri;
            DateTimeOffset? lastTrackPointTime = data.LastTrackPointTime;

            try
            {
                LiveTrackQueryResult queryResult =
                    await this.backendWebApi.GetLiveTrackDataAsync(
                        liveTrackId,
                        lastTrackPointTime);

                return new JsonResult(queryResult);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }
    }
}
