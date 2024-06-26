﻿using SharpKml.Dom;
using SharpKml.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;
using TimeSpan = System.TimeSpan;

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
        /// https://explore.garmin.com/Social
        /// </summary>
        private const string InreachServiceUrl = "https://explore.garmin.com/feed/Share/{0}";

        /// <summary>
        /// Web page URL for MapShare
        /// </summary>
        private const string MapSharePublicUrl = "https://share.garmin.com/{0}";
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// Minimum distance in time span between two requests to web service
        /// </summary>
        private static readonly TimeSpan MinRequestDistance = TimeSpan.FromMinutes(1.0);

        /// <summary>
        /// The minimum tracking time that a Garmin inReach user can have
        /// </summary>
        private static readonly TimeSpan MinTrackingInterval = TimeSpan.FromMinutes(10.0);

        /// <summary>
        /// HTTP client used for requests
        /// </summary>
        private readonly HttpClient client = new();

        /// <summary>
        /// Mapping of MapShare identifier to Date/time of last request
        /// </summary>
        private readonly Dictionary<string, DateTimeOffset> lastRequestByMapShareIdentifier = [];

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
                ? this.lastRequest.Value + MinRequestDistance
                : DateTimeOffset.Now;

            if (this.lastRequestByMapShareIdentifier.TryGetValue(
                mapShareIdentifier,
                out var lastRequestDate))
            {
                var nextUserRequest = lastRequestDate + MinTrackingInterval;
                return nextUserRequest < nextSystemWideRequest ? nextSystemWideRequest : nextUserRequest;
            }

            return nextSystemWideRequest;
        }

        /// <summary>
        /// Gets live waypoint data for Garmin inReach device, using the MapShare identifier given
        /// </summary>
        /// <param name="mapShareIdentifier">MapShare identifier</param>
        /// <param name="password">passwort for accessing MapShare data; may be null</param>
        /// <returns>live waypoint data for device</returns>
        public async Task<LiveWaypointData> GetDataAsync(
            string mapShareIdentifier,
            string? password = null)
        {
            string requestUrl = string.Format(InreachServiceUrl, mapShareIdentifier);

            var stream =
                string.IsNullOrEmpty(password)
                ? await this.client.GetStreamAsync(requestUrl)
                : await this.GetStreamWithBasicAuth(
                    requestUrl,
                    string.Empty,
                    password!);

            var thisRequestTime = DateTimeOffset.Now;
            this.lastRequest = thisRequestTime;
            this.lastRequestByMapShareIdentifier[mapShareIdentifier] = thisRequestTime;

            return this.ParseRawKmlFileWaypointData(stream, mapShareIdentifier);
        }

        /// <summary>
        /// Gets live track data for Garmin inReach device, using the MapShare identifier given
        /// </summary>
        /// <param name="mapShareIdentifier">MapShare identifier</param>
        /// <param name="startTime">
        /// indicates the starting date and time of the data queried from the Garmin server
        /// </param>
        /// <param name="password">passwort for accessing MapShare data; may be null</param>
        /// <returns>live track data for device</returns>
        public async Task<LiveTrackData> GetTrackAsync(
            string mapShareIdentifier,
            DateTimeOffset startTime,
            string? password = null)
        {
            string requestUrl = string.Format(InreachServiceUrl, mapShareIdentifier);
            requestUrl += "?d1=" + startTime.UtcDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mmK");

            var stream =
                string.IsNullOrEmpty(password)
                ? await this.client.GetStreamAsync(requestUrl)
                : await this.GetStreamWithBasicAuth(
                    requestUrl,
                    string.Empty,
                    password!);

            var thisRequestTime = DateTimeOffset.Now;
            this.lastRequest = thisRequestTime;
            this.lastRequestByMapShareIdentifier[mapShareIdentifier] = thisRequestTime;

            return ParseRawKmlFileTrackData(stream, mapShareIdentifier);
        }

        /// <summary>
        /// Returns a stream to get MapShare infos with username and password
        /// </summary>
        /// <param name="requestUrl">request URL to send</param>
        /// <param name="username">username to use</param>
        /// <param name="password">password to use</param>
        /// <returns>stream object</returns>
        private async Task<Stream> GetStreamWithBasicAuth(
            string requestUrl,
            string username,
            string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            string authenticationString = $"{username}:{password}";

            string base64EncodedAuthenticationString = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes(authenticationString));

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Basic",
                    base64EncodedAuthenticationString);

            var response = await this.client.SendAsync(request);
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Parses the raw KML Data file returned by the inReach service and produces live
        /// waypoint data.
        /// </summary>
        /// <param name="stream">stream to read kml from</param>
        /// <param name="mapShareIdentifier">MapShare identifier used for request</param>
        /// <returns>live waypoint data</returns>
        internal LiveWaypointData ParseRawKmlFileWaypointData(Stream stream, string mapShareIdentifier)
        {
            var file = KmlFile.Load(stream);

            if (file.Root.Flatten().FirstOrDefault(x => x is Placemark) is not Placemark placemark)
            {
                throw new FormatException("No Garmin inReach position/placemark returned from the server");
            }

            var point = placemark.Geometry as Point;

            if (point?.Coordinate == null)
            {
                throw new FormatException("Couldn't find Point geometry in Garmin inReach KML returned from the server");
            }

            var when = (placemark.Time as Timestamp)?.When;

            if (when != null)
            {
                this.lastRequestByMapShareIdentifier[mapShareIdentifier] = new DateTimeOffset(when.Value);
            }

            return new LiveWaypointData(FormatLiveWaypointId(mapShareIdentifier))
            {
                Name = "Garmin inReach " + placemark.Name,
                Latitude = point.Coordinate.Latitude,
                Longitude = point.Coordinate.Longitude,
                Altitude = (int)(point.Coordinate.Altitude ?? 0.0),
                TimeStamp = when.HasValue ? new DateTimeOffset(when.Value) : DateTimeOffset.Now,
                Description = FormatDescriptionFromPlacemark(placemark) +
                    LiveWaypointCacheManager.GetCoveredDistanceDescription(
                        new MapPoint(
                            point.Coordinate.Latitude,
                            point.Coordinate.Longitude)),
                DetailsLink = string.Format(MapSharePublicUrl, mapShareIdentifier),
            };
        }

        /// <summary>
        /// Parses the raw KML Data file returned by the inReach service and produces live
        /// track data.
        /// </summary>
        /// <param name="stream">stream to read kml from</param>
        /// <param name="mapShareIdentifier">MapShare identifier used for request</param>
        /// <returns>live track data</returns>
        internal static LiveTrackData ParseRawKmlFileTrackData(
            Stream stream,
            string mapShareIdentifier)
        {
            var file = KmlFile.Load(stream);

            // find all point placemarks and parse their extra data for date/time
            // ignore the LineString placemarks as they have no time reference
            var allPointPlacemarks =
                file.Root.Flatten()
                .Where(
                    element => element is Placemark placemark &&
                    placemark.Geometry is Point);

            var firstPlacemark = allPointPlacemarks.FirstOrDefault() as Placemark;

            var lastLineStringPlacemark =
                file.Root.Flatten()
                .LastOrDefault(element => element is Placemark placemark &&
                placemark.Geometry is LineString) as Placemark;

            if (firstPlacemark == null ||
                lastLineStringPlacemark == null)
            {
                throw new FormatException("No Garmin inReach Point placemarks returned from the server");
            }

            var track = new Track(Guid.NewGuid().ToString("B"));

            foreach (Placemark placemark in allPointPlacemarks.Cast<Placemark>())
            {
                if (placemark.Geometry is Point point)
                {
                    TrackPoint trackPoint =
                        GetTrackPointFromKmlPointGeometry(placemark, point);

                    track.TrackPoints.Add(trackPoint);
                }
            }

            DateTimeOffset? trackStart = track.TrackPoints.FirstOrDefault()?.Time;

            if (trackStart == null)
            {
                throw new FormatException("No Garmin inReach track points with time references found");
            }

            var trackPoints = track.TrackPoints
                .Where(trackPoint => trackPoint.Time.HasValue)
                .Select(
                trackPoint => new LiveTrackData.LiveTrackPoint
                {
                    Latitude = trackPoint.Latitude,
                    Longitude = trackPoint.Longitude,
                    Altitude = trackPoint.Altitude ?? 0.0,
                    Offset = (trackPoint.Time!.Value - trackStart.Value).TotalSeconds,
                });

            return new LiveTrackData(
                FormatLiveWaypointId(mapShareIdentifier),
                "Garmin inReach " + lastLineStringPlacemark.Name)
            {
                Description = lastLineStringPlacemark.Description.Text,
                TrackStart = trackStart.Value,
                TrackPoints = trackPoints.ToArray(),
            };
        }

        /// <summary>
        /// Creates track point object from SharpKml Point object with Garmin extra data
        /// </summary>
        /// <param name="placemark">point placemark</param>
        /// <param name="point">point object</param>
        /// <returns>track point object</returns>
        private static TrackPoint GetTrackPointFromKmlPointGeometry(Placemark placemark, Point point)
        {
            var timestamp = placemark?.Time as Timestamp;
            var timeUtc = timestamp?.When;
            DateTimeOffset? time = timeUtc != null
                ? new DateTimeOffset(timeUtc.Value)
                : null;

            return new TrackPoint(
                point.Coordinate.Latitude,
                point.Coordinate.Longitude,
                point.Coordinate.Altitude,
                heading: null)
            {
                Time = time,
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
                placemark.ExtendedData.Data.ToDictionary(
                    data => data.Name,
                    data => data.Value);

            string inEmergency =
                extendedData.TryGetValue(
                    "In Emergency",
                    out string? value)
                ? value
                : "Unknown";

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
                GetValueOrDefault(extendedData, "Time UTC", "N/A"),
                GetValueOrDefault(extendedData, "Device Type", "N/A"),
                GetValueOrDefault(extendedData, "Velocity", "N/A"),
                GetValueOrDefault(extendedData, "Course", "N/A"),
                GetValueOrDefault(extendedData, "Event", "N/A"),
                GetValueOrDefault(extendedData, "Text", "N/A"));
        }

        /// <summary>
        /// Returns a dictionary value by key, or the default value when key was not found.
        /// </summary>
        /// <param name="dict">dictionary to use</param>
        /// <param name="key">key value</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>found value or default</returns>
        private static TValue? GetValueOrDefault<TKey, TValue>(
            Dictionary<TKey, TValue> dict,
            TKey key,
            TValue? defaultValue = default)
            where TKey : notnull
        {
            return dict.TryGetValue(key, out TValue? value)
                ? value
                : defaultValue;
        }
    }
}
