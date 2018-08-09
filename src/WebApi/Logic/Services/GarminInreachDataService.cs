using SharpKml.Dom;
using SharpKml.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
        /// HTTP client used for requests
        /// </summary>
        private readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Gets live waypoint data for Garmin inREach device, using the MapShare identifier given
        /// </summary>
        /// <param name="mapShareIdentifier">MapShare identifier</param>
        /// <returns>live waypoint data for device</returns>
        public async Task<LiveWaypointData> GetDataAsync(string mapShareIdentifier)
        {
            string requestUrl = string.Format(InreachServiceUrl, mapShareIdentifier);

            var stream = await this.client.GetStreamAsync(requestUrl);

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

            var placemark = file.Root.Flatten().First(x => x is Placemark) as Placemark;

            if (placemark == null)
            {
                throw new FormatException("Garmin inReach Raw KML Data contains no Placemark");
            }

            var point = placemark.Geometry as Point;

            var when = (placemark.Time as Timestamp).When;

            return new LiveWaypointData
            {
                ID = FormatLiveWaypointId(mapShareIdentifier),
                Name = "Garmin inReach " + placemark.Name,
                Latitude = point.Coordinate.Latitude,
                Longitude = point.Coordinate.Longitude,
                Altitude = (int)(point.Coordinate.Altitude ?? 0.0),
                TimeStamp = when.HasValue ? new DateTimeOffset(when.Value) : DateTimeOffset.Now,
                Description = FormatDescriptionFromPlacemark(placemark),
                DetailsLink = string.Format(MapSharePublicUrl, mapShareIdentifier),
            };
        }

        /// <summary>
        /// Formats Live Waypoint ID from MapShare identifier
        /// </summary>
        /// <param name="mapShareIdentifier">MapShare identifier to use</param>
        /// <returns>Live Waypoint ID</returns>
        private static string FormatLiveWaypointId(string mapShareIdentifier)
        {
            // TODO use LiveWaypointID class
            return "wheretofly-inreach-" + mapShareIdentifier;
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
                .Replace("True", "YES!")
                .Replace("true", "YES");

            return string.Format(
                "In Emergency: {0}\n" +
                "Time in UTC: {1}\n" +
                "Device type: {2}\n" +
                "Velocity: {3}\n" +
                "Course: {4}\n" +
                "Event: {5}\n" +
                "Text: {6}",
                inEmergency,
                ValueOrDefault(extendedData, "Time UTC", "N/A"),
                ValueOrDefault(extendedData, "Device Type", "N/A"),
                ValueOrDefault(extendedData, "Velocity", "N/A"),
                ValueOrDefault(extendedData, "Course", "N/A"),
                ValueOrDefault(extendedData, "Event", "N/A"),
                ValueOrDefault(extendedData, "Text", "N/A"));
        }

        /// <summary>
        /// Gets value from dictionary or default value, when key doesn't exist
        /// </summary>
        /// <param name="dict">dictionary to use</param>
        /// <param name="key">key to find</param>
        /// <param name="defaultValue">default value to return</param>
        /// <returns>value in the dictionary, or default value</returns>
        private static string ValueOrDefault(Dictionary<string, string> dict, string key, string defaultValue)
        {
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }
    }
}
