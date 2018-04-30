using System;
using System.Collections.Generic;
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
        private static string gpxNamespace = "http://www.topografix.com/GPX/1/1";

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
            namespaceManager.AddNamespace("x", gpxNamespace);

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1113:CommaMustBeOnSameLineAsPreviousParameter",
            Justification = "False positive")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1117:ParametersMustBeOnSameLineOrSeparateLines",
            Justification = "False positive")]
        private static Location GetLocationFromWaypointNode(XmlNode waypointNode, XmlNamespaceManager namespaceManager)
        {
            bool canParseLatitude = double.TryParse(
                waypointNode.Attributes["lat"].Value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out double latitude);

            bool canParseLongitude = double.TryParse(
                waypointNode.Attributes["lon"].Value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out double longitude);

            if (!canParseLatitude || !canParseLongitude)
            {
                throw new FormatException("missing lat or long attributes on wpt element in gpx file");
            }

            double elevation = 0;
            XmlNode elevationNode = waypointNode.SelectSingleNode("x:ele", namespaceManager);
            if (elevationNode != null &&
                !string.IsNullOrEmpty(elevationNode.InnerText))
            {
                double.TryParse(elevationNode.InnerText, out elevation);
            }

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
            }

            return LocationType.Waypoint;
        }
    }
}
