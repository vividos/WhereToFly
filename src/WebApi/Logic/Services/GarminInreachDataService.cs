using SharpKml.Dom;
using SharpKml.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WhereToFly.Shared.Base;
using WhereToFly.Shared.Model;

namespace WhereToFly.WebApi.Logic.Services
{
    /// <summary>
    /// Data service for Garmin inReach devices; documentation at:
    /// https://files.delorme.com/support/inreachwebdocs/KML%20Feeds.pdf
    /// </summary>
    public class GarminInreachDataService
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        /// <summary>
        /// Service URL for inReach service; single parameter is the MapShare identifier which
        /// can be configured on the "Social" tab of the inReach page, at
        /// https://inreach.garmin.com/Social
        /// </summary>
        private const string InreachServiceUrl = "https://inreach.garmin.com/feed/Share/{0}";

        /// <summary>
        /// Web page URL for MapShare
        /// </summary>
        private const string MapSharePublicUrl = "https://share.garmin.com/{0}";
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// Minimum distance in time span between two requests to web service
        /// </summary>
        private static System.TimeSpan minRequestDistance = System.TimeSpan.FromMinutes(1.0);

        /// <summary>
        /// The minimum tracking time that a Garmin inReach user can have
        /// </summary>
        private static System.TimeSpan minTrackingInterval = System.TimeSpan.FromMinutes(10.0);

        /// <summary>
        /// HTTP client used for requests
        /// </summary>
        private readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Mapping of MapShare identifier to Date/time of last request
        /// </summary>
        private readonly Dictionary<string, DateTimeOffset> lastRequestByMapShareIdentifier = new Dictionary<string, DateTimeOffset>();

        /// <summary>
        /// Date/time of last request, or null when no request was made yet
        /// </summary>
        private DateTimeOffset? lastRequest = null;

        /// <summary>
        /// Returns next possible request date for given MapShare identifier
        /// </summary>
        /// <param name="mapShareIdentifier">MapShare identifier</param>
        /// <returns>next possible request date</returns>
        public DateTimeOffset GetNextRequestDate(string mapShareIdentifier)
        {
            DateTimeOffset nextSystemWideRequest = this.lastRequest.HasValue
                ? this.lastRequest.Value + minRequestDistance
                : DateTimeOffset.Now;

            if (this.lastRequestByMapShareIdentifier.ContainsKey(mapShareIdentifier))
            {
                var nextUserRequest = this.lastRequestByMapShareIdentifier[mapShareIdentifier] + minTrackingInterval;
                return nextUserRequest < nextSystemWideRequest ? nextSystemWideRequest : nextUserRequest;
            }

            return nextSystemWideRequest;
        }

        /// <summary>
        /// Gets live waypoint data for Garmin inREach device, using the MapShare identifier given
        /// </summary>
        /// <param name="mapShareIdentifier">MapShare identifier</param>
        /// <returns>live waypoint data for device</returns>
        public async Task<LiveWaypointData> GetDataAsync(string mapShareIdentifier)
        {
            string requestUrl = string.Format(InreachServiceUrl, mapShareIdentifier);

            var stream = await this.client.GetStreamAsync(requestUrl);

            var thisRequestTime = DateTimeOffset.Now;
            this.lastRequest = thisRequestTime;
            this.lastRequestByMapShareIdentifier[mapShareIdentifier] = thisRequestTime;

            return this.ParseRawKmlDataFile(stream, mapShareIdentifier);
        }

        /// <summary>
        /// Parses the raw KML Data file returned by the inReach service and produces live
        /// waypoint data.
        /// </summary>
        /// <param name="stream">stream to read kml from</param>
        /// <param name="mapShareIdentifier">MapShare identifier used for request</param>
        /// <returns>live waypoint data</returns>
        internal LiveWaypointData ParseRawKmlDataFile(Stream stream, string mapShareIdentifier)
        {
            var file = KmlFile.Load(stream);

            var placemark = file.Root.Flatten().FirstOrDefault(x => x is Placemark) as Placemark;

            if (placemark == null)
            {
                throw new FormatException("No Garmin inReach position/placemark returned from the server");
            }

            var point = placemark.Geometry as Point;

            var when = (placemark.Time as Timestamp).When;

            if (when.HasValue)
            {
                this.lastRequestByMapShareIdentifier[mapShareIdentifier] = new DateTimeOffset(when.Value);
            }

            return new LiveWaypointData
            {
                ID = FormatLiveWaypointId(mapShareIdentifier),
                Name = "Garmin inReach " + placemark.Name,
                Latitude = point.Coordinate.Latitude,
                Longitude = point.Coordinate.Longitude,
                Altitude = (int)(point.Coordinate.Altitude ?? 0.0),
                TimeStamp = when.HasValue ? new DateTimeOffset(when.Value) : DateTimeOffset.Now,
                Description = FormatDescriptionFromPlacemark(placemark) +
                    LiveWaypointCacheManager.GetCoveredDistanceDescription(new MapPoint(point.Coordinate.Latitude, point.Coordinate.Longitude)),
                DetailsLink = string.Format(MapSharePublicUrl, mapShareIdentifier),
            };
        }

        /// <summary>
        /// Formats app resource URI from MapShare identifier
        /// </summary>
        /// <param name="mapShareIdentifier">MapShare identifier to use</param>
        /// <returns>Live Waypoint ID</returns>
        private static string FormatLiveWaypointId(string mapShareIdentifier)
        {
            return new AppResourceUri(AppResourceUri.ResourceType.GarminInreachPos, mapShareIdentifier).ToString();
        }

        /// <summary>
        /// Formats a human-readable description from given placemark, containing extended data
        /// from the track point.
        /// </summary>
        /// <param name="placemark">placemark to use</param>
        /// <returns>description text</returns>
        private static string FormatDescriptionFromPlacemark(Placemark placemark)
        {
            var extendedData =
                placemark.ExtendedData.Data.ToDictionary(data => data.Name, data => data.Value);

            string inEmergency =
                extendedData.ContainsKey("In Emergency") ? extendedData["In Emergency"] : "Unknown";

            inEmergency = inEmergency.Replace("False", "no")
                .Replace("false", "no")
                .Replace("True", "<b style='color: red'>YES!</b>")
                .Replace("true", "<b style='color: red'>YES!</b>");

            return string.Format(
                "In Emergency: {0}<br/>" +
                "Time in UTC: {1}<br/>" +
                "Device type: {2}<br/>" +
                "Velocity: {3}<br/>" +
                "Course: {4}<br/>" +
                "Event: {5}<br/>" +
                "Text: {6}",
                inEmergency,
                extendedData.GetValueOrDefault("Time UTC", "N/A"),
                extendedData.GetValueOrDefault("Device Type", "N/A"),
                extendedData.GetValueOrDefault("Velocity", "N/A"),
                extendedData.GetValueOrDefault("Course", "N/A"),
                extendedData.GetValueOrDefault("Event", "N/A"),
                extendedData.GetValueOrDefault("Text", "N/A"));
        }
    }
}
