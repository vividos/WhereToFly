using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo.DataFormats
{
    /// <summary>
    /// Data file for loading content from GPX file. See GPX specification:
    /// http://www.topografix.com/gpx.asp
    /// </summary>
    internal class GpxDataFile : IGeoDataFile
    {
        /// <summary>
        /// GPX 1.0 namespace to use
        /// </summary>
        private const string Gpx10Namespace = "http://www.topografix.com/GPX/1/0";

        /// <summary>
        /// GPX 1.1 namespace to use
        /// </summary>
        private const string Gpx11Namespace = "http://www.topografix.com/GPX/1/1";

        /// <summary>
        /// GPX xml document
        /// </summary>
        private readonly XmlDocument gpxDocument;

        /// <summary>
        /// Namespace manager for GPX namespace
        /// </summary>
        private readonly XmlNamespaceManager namespaceManager;

        /// <summary>
        /// Creates a new GPX data file
        /// </summary>
        /// <param name="stream">stream containing GPX file</param>
        public GpxDataFile(Stream stream)
        {
            this.gpxDocument = new XmlDocument();
            this.gpxDocument.Load(stream);

            bool useVersion11 = this.CheckGpx11Version();

            this.namespaceManager = new XmlNamespaceManager(this.gpxDocument.NameTable);
            this.namespaceManager.AddNamespace("x", useVersion11 ? Gpx11Namespace : Gpx10Namespace);
        }

        /// <summary>
        /// Checks if the GPX xml document has version 1.0 or 1.1.
        /// </summary>
        /// <returns>true when version 1.1, false when 1.0 or any other</returns>
        private bool CheckGpx11Version()
        {
            string version = this.gpxDocument.DocumentElement.GetAttribute("version");

            return version == null ||
                version == "1.1";
        }

        /// <summary>
        /// Returns if the opened file contains any locations
        /// </summary>
        /// <returns>true when the file contains locations, false when not</returns>
        public bool HasLocations()
        {
            XmlNodeList waypointNodeList = this.gpxDocument.SelectNodes("//x:wpt", this.namespaceManager);

            return waypointNodeList.Count > 0;
        }

        /// <summary>
        /// Returns a list of tracks contained in the gpx file
        /// </summary>
        /// <returns>list of tracks found in the file</returns>
        public List<string> GetTrackList()
        {
            XmlNodeList trackNodeList = this.gpxDocument.SelectNodes("//x:trk", this.namespaceManager);

            var trackList = new List<string>();

            foreach (XmlNode trackNode in trackNodeList)
            {
                XmlNode nameNode = trackNode.SelectSingleNode("x:name", this.namespaceManager);

                string name = nameNode?.InnerText ?? "Track";

                trackList.Add(name);
            }

            return trackList;
        }

        /// <summary>
        /// Loads track with given index from GPX file
        /// </summary>
        /// <param name="trackIndex">index of track to load</param>
        /// <returns>loaded track</returns>
        public Track LoadTrack(int trackIndex)
        {
            XmlNodeList trackNodeList = this.gpxDocument.SelectNodes("//x:trk", this.namespaceManager);

            if (trackIndex >= trackNodeList.Count)
            {
                throw new FormatException("invalid track index in GPX file");
            }

            return this.LoadTrackFromTrackNode(trackNodeList[trackIndex]);
        }

        /// <summary>
        /// Loads track from given track node
        /// </summary>
        /// <param name="trackNode">track xml node</param>
        /// <returns>loaded track</returns>
        private Track LoadTrackFromTrackNode(XmlNode trackNode)
        {
            XmlNode nameNode = trackNode.SelectSingleNode("x:name", this.namespaceManager);

            string name = nameNode?.InnerText ?? "Track";

            var track = new Track
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = name,
            };

            foreach (XmlNode trackSegmentNode in trackNode.SelectNodes("x:trkseg", this.namespaceManager))
            {
                foreach (XmlNode trackPointNode in trackSegmentNode.SelectNodes("x:trkpt", this.namespaceManager))
                {
                    track.TrackPoints.Add(GetTrackPointFromTrackPointNode(trackPointNode, this.namespaceManager));
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

            return new TrackPoint(latitude, longitude, elevation, heading: null)
            {
                Time = ParseTime(trackPointNode, namespaceManager),
            };
        }

        /// <summary>
        /// Loads location list from GPX file. The Waypoint items of the GPX file are returned.
        /// </summary>
        /// <returns>list of waypoint locations in file</returns>
        public List<Location> LoadLocationList()
        {
            XmlNodeList waypointNodeList = this.gpxDocument.SelectNodes("//x:wpt", this.namespaceManager);

            var locationList = new List<Location>();

            foreach (XmlNode waypointNode in waypointNodeList)
            {
                locationList.Add(GetLocationFromWaypointNode(waypointNode, this.namespaceManager));
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
                MapLocation = new MapPoint(latitude, longitude, elevation),
                Description = descNode?.InnerText ?? string.Empty,
                Type = LocationTypeFromWaypointNode(nameNode),
                InternetLink = linkHrefNode?.Value ?? string.Empty,
            };

            return location;
        }

        /// <summary>
        /// Parses "lat" and "long" attributes on a waypoint or track point node
        /// </summary>
        /// <param name="node">wpt or trkpt node to parse</param>
        /// <param name="latitude">parsed latitude value</param>
        /// <param name="longitude">parsed longitude value</param>
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
