using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using WhereToFly.App.Logic;
using WhereToFly.App.Model;

namespace WhereToFly.App.Geo.DataFormats
{
    /// <summary>
    /// Loader for GPX format. See GPX specification: http://www.topografix.com/gpx.asp
    /// </summary>
    internal static class GpxFormatLoader
    {
        /// <summary>
        /// GPX namespace to use
        /// </summary>
        private static readonly string GpxNamespace = "http://www.topografix.com/GPX/1/1";

        /// <summary>
        /// Returns a list of tracks contained in the given gpx file stream.
        /// </summary>
        /// <param name="stream">stream containing GPX file</param>
        /// <returns>list of tracks found in the file</returns>
        public static List<string> GetTrackList(Stream stream)
        {
            XmlDocument gpxDocument = new XmlDocument();
            gpxDocument.Load(stream);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(gpxDocument.NameTable);
            namespaceManager.AddNamespace("x", GpxNamespace);

            XmlNodeList trackNodeList = gpxDocument.SelectNodes("//x:trk", namespaceManager);

            var trackList = new List<string>();

            foreach (XmlNode trackNode in trackNodeList)
            {
                XmlNode nameNode = trackNode.SelectSingleNode("x:name", namespaceManager);

                string name = nameNode?.InnerText ?? "Track";

                trackList.Add(name);
            }

            return trackList;
        }

        /// <summary>
        /// Loads track with given index from GPX file specified by stream
        /// </summary>
        /// <param name="stream">stream containing GPX file</param>
        /// <param name="trackIndex">index of track to load</param>
        /// <returns>loaded track</returns>
        public static Track LoadTrack(Stream stream, int trackIndex)
        {
            XmlDocument gpxDocument = new XmlDocument();
            gpxDocument.Load(stream);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(gpxDocument.NameTable);
            namespaceManager.AddNamespace("x", GpxNamespace);

            XmlNodeList trackNodeList = gpxDocument.SelectNodes("//x:trk", namespaceManager);

            if (trackIndex >= trackNodeList.Count)
            {
                throw new FormatException("invalid track index in GPX file");
            }

            return LoadTrackFromTrackNode(trackNodeList[trackIndex], namespaceManager);
        }

        /// <summary>
        /// Loads track from given track node
        /// </summary>
        /// <param name="trackNode">track xml node</param>
        /// <param name="namespaceManager">xml namespace manager to use</param>
        /// <returns>loaded track</returns>
        private static Track LoadTrackFromTrackNode(XmlNode trackNode, XmlNamespaceManager namespaceManager)
        {
            XmlNode nameNode = trackNode.SelectSingleNode("x:name", namespaceManager);

            string name = nameNode?.InnerText ?? "Track";

            var track = new Track
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = name
            };

            foreach (XmlNode trackSegmentNode in trackNode.SelectNodes("x:trkseg", namespaceManager))
            {
                foreach (XmlNode trackPointNode in trackSegmentNode.SelectNodes("x:trkpt", namespaceManager))
                {
                    track.TrackPoints.Add(GetTrackPointFromTrackPointNode(trackPointNode, namespaceManager));
                }
            }

            return track;
        }

        /// <summary>
        /// Gets TrackPoint object from given track point ("trkpt") node
        /// </summary>
        /// <param name="trackPointNode">track point node to use</param>
        /// <param name="namespaceManager">xml namespace manager to use</param>
        /// <returns>track point object</returns>
        private static TrackPoint GetTrackPointFromTrackPointNode(XmlNode trackPointNode, XmlNamespaceManager namespaceManager)
        {
            // GPX xml to load:
            // <trkpt lat="46.029650000855327" lon="11.817330038174987">
            //   <ele>1435</ele>
            //   <time>2018-07-26T08:43:13Z</time>
            // </trkpt>
            // ele and time elements are optional
            ParseLatLongAttributes(trackPointNode, out double latitude, out double longitude);

            double elevation = ParseElevation(trackPointNode, namespaceManager);
            int altitude = (int)elevation;

            return new TrackPoint(latitude, longitude, altitude, heading: null)
            {
                Time = ParseTime(trackPointNode, namespaceManager)
            };
        }

        /// <summary>
        /// Loads location list from given stream containing a GPX file. The Waypoint items of the
        /// GPX file is returned.
        /// </summary>
        /// <param name="stream">stream containing GPX file</param>
        /// <returns>list of waypoint locations in file</returns>
        public static List<Location> LoadLocationList(Stream stream)
        {
            XmlDocument gpxDocument = new XmlDocument();
            gpxDocument.Load(stream);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(gpxDocument.NameTable);
            namespaceManager.AddNamespace("x", GpxNamespace);

            XmlNodeList waypointNodeList = gpxDocument.SelectNodes("//x:wpt", namespaceManager);

            var locationList = new List<Location>();

            foreach (XmlNode waypointNode in waypointNodeList)
            {
                locationList.Add(GetLocationFromWaypointNode(waypointNode, namespaceManager));
            }

            return locationList;
        }

        /// <summary>
        /// Creates a location object from given GPX waypoint node.
        /// </summary>
        /// <param name="waypointNode">waypoint xml node</param>
        /// <param name="namespaceManager">xml namespace manager to use</param>
        /// <returns>location object</returns>
        private static Location GetLocationFromWaypointNode(XmlNode waypointNode, XmlNamespaceManager namespaceManager)
        {
            ParseLatLongAttributes(waypointNode, out double latitude, out double longitude);

            double elevation = ParseElevation(waypointNode, namespaceManager);

            XmlNode nameNode = waypointNode.SelectSingleNode("x:name", namespaceManager);
            XmlNode descNode = waypointNode.SelectSingleNode("x:desc", namespaceManager);
            XmlNode linkHrefNode = waypointNode.SelectSingleNode("x:link/@href", namespaceManager);

            var location = new Location
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = nameNode?.InnerText ?? "Waypoint",
                Elevation = elevation,
                MapLocation = new MapPoint(latitude, longitude),
                Description = HtmlConverter.Sanitize(descNode?.InnerText ?? string.Empty),
                Type = LocationTypeFromWaypointNode(nameNode),
                InternetLink = linkHrefNode?.Value ?? string.Empty
            };

            return location;
        }

        /// <summary>
        /// Parses "lat" and "long" attributes on a waypoint or track point node
        /// </summary>
        /// <param name="node">wpt or trkpt node to parse</param>
        /// <param name="latitude">parsed latitude value</param>
        /// <param name="longitude">parsed longitude value</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1113:CommaMustBeOnSameLineAsPreviousParameter",
            Justification = "False positive")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1117:ParametersMustBeOnSameLineOrSeparateLines",
            Justification = "False positive")]
        private static void ParseLatLongAttributes(XmlNode node, out double latitude, out double longitude)
        {
            bool canParseLatitude = double.TryParse(
                node.Attributes["lat"].Value,
                NumberStyles.Float,
                NumberFormatInfo.InvariantInfo,
                out latitude);

            bool canParseLongitude = double.TryParse(
                node.Attributes["lon"].Value,
                NumberStyles.Float,
                NumberFormatInfo.InvariantInfo,
                out longitude);

            if (!canParseLatitude || !canParseLongitude)
            {
                throw new FormatException("missing lat or long attributes on wpt or trkpt element in gpx file");
            }
        }

        /// <summary>
        /// Parses elevation attribute in waypoint or track point node, when available
        /// </summary>
        /// <param name="node">waypoint node to check</param>
        /// <param name="namespaceManager">namespace manager</param>
        /// <returns>parsed elevation value, or 0.0</returns>
        private static double ParseElevation(XmlNode node, XmlNamespaceManager namespaceManager)
        {
            double elevation = 0.0;
            XmlNode elevationNode = node.SelectSingleNode("x:ele", namespaceManager);
            if (elevationNode != null &&
                !string.IsNullOrEmpty(elevationNode.InnerText) &&
                double.TryParse(elevationNode.InnerText, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out elevation))
            {
                elevation = Math.Round(elevation, 1);
            }

            return elevation;
        }

        /// <summary>
        /// Parses time attribute in waypoint node, when available
        /// </summary>
        /// <param name="trackPointNode">track point node to check</param>
        /// <param name="namespaceManager">namespace manager</param>
        /// <returns>parsed elevation value, or 0.0</returns>
        private static DateTimeOffset? ParseTime(XmlNode trackPointNode, XmlNamespaceManager namespaceManager)
        {
            XmlNode timeNode = trackPointNode.SelectSingleNode("x:time", namespaceManager);
            if (timeNode != null &&
                !string.IsNullOrEmpty(timeNode.InnerText) &&
                DateTimeOffset.TryParse(timeNode.InnerText, NumberFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out DateTimeOffset time))
            {
                return time;
            }

            return null;
        }

        /// <summary>
        /// Determines a location type from given name of waypoint node.
        /// </summary>
        /// <param name="nameNode">name node</param>
        /// <returns>waypoint type</returns>
        private static LocationType LocationTypeFromWaypointNode(XmlNode nameNode)
        {
            // check for some name contents in the DHV Geländedatenbank
            if (nameNode != null &&
                !string.IsNullOrEmpty(nameNode.InnerText))
            {
                if (nameNode.InnerText.Contains("Startplatz"))
                {
                    return LocationType.FlyingTakeoff;
                }

                if (nameNode.InnerText.Contains("Landeplatz"))
                {
                    return LocationType.FlyingLandingPlace;
                }

                if (nameNode.InnerText.StartsWith("TP") ||
                    nameNode.InnerText.StartsWith("XCP"))
                {
                    return LocationType.Turnpoint;
                }
            }

            return LocationType.Waypoint;
        }
    }
}
