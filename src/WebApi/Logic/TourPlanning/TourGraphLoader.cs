using SharpKml.Dom;
using SharpKml.Dom.GX;
using SharpKml.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Base;

namespace WhereToFly.WebApi.Logic.TourPlanning
{
    /// <summary>
    /// Tour graph loader; loads a kml file with waypoints and tracks that contain extra meta data
    /// in the description fields. The meta data describes relationships between waypoints and
    /// tracks and build a directed graph of tracks between waypoints. The PlanTourEngine can then
    /// use the graph to plan tours.
    /// </summary>
    internal class TourGraphLoader
    {
        /// <summary>
        /// Key for ID field of waypoint and track
        /// </summary>
        private const string KeyId = "ID";

        /// <summary>
        /// Key for description field of waypoint and track
        /// </summary>
        private const string KeyDescription = "DESC";

        /// <summary>
        /// Key for FROM field of track
        /// </summary>
        private const string KeyFrom = "FROM";

        /// <summary>
        /// Key for TO field of track
        /// </summary>
        private const string KeyTo = "TO";

        /// <summary>
        /// Key for reverse description field of track
        /// </summary>
        private const string KeyReverseDesc = "REVDESC";

        /// <summary>
        /// Key for duration field of track
        /// </summary>
        private const string KeyDuration = "DURATION";

        /// <summary>
        /// Key for reverse duration field of track
        /// </summary>
        private const string KeyReverseDuration = "REVDURATION";

        /// <summary>
        /// Tour engine to fill graph
        /// </summary>
        private readonly PlanTourEngine tourEngine;

        /// <summary>
        /// Creates a new tour graph loader object
        /// </summary>
        /// <param name="tourEngine">tour engine to fill graph with</param>
        public TourGraphLoader(PlanTourEngine tourEngine)
        {
            this.tourEngine = tourEngine;
        }

        /// <summary>
        /// Loads a .kml file from stream
        /// </summary>
        /// <param name="kmlStream">stream containing a .kml file</param>
        public void Load(Stream kmlStream)
        {
            using (var reader = new StreamReader(kmlStream))
            {
                var kml = KmlFile.Load(reader);
                this.ParseKmlFile(kml);
            }
        }

        /// <summary>
        /// Parses a kml file for waypoints and tracks and adds them to the tour engine
        /// </summary>
        /// <param name="kml">kml file to load</param>
        private void ParseKmlFile(KmlFile kml)
        {
            if (kml.Root is Kml kmlRoot &&
                kmlRoot.Feature is Document kmlDocument)
            {
                // the file must have two folders at its root, with waypoints and tracks
                var waypointsFolder = kmlDocument.Features.First(
                    x => x is Folder folder && (folder.Name == "Wegpunkte" || folder.Name == "Waypoints"));

                if (waypointsFolder != null)
                {
                    this.LoadWaypoints(waypointsFolder as Folder);
                }

                var trackFolder = kmlDocument.Features.First(x => x is Folder folder && folder.Name == "Tracks");

                if (trackFolder != null)
                {
                    this.LoadTracks(trackFolder as Folder);
                }
            }
        }

        /// <summary>
        /// Loads all waypoints from folder and adds them to the tour engine.
        /// </summary>
        /// <param name="waypointsFolder">folder to load from</param>
        private void LoadWaypoints(Folder waypointsFolder)
        {
            foreach (var item in waypointsFolder.Flatten())
            {
                if (item is Placemark placemark)
                {
                    var waypointInfo = WaypointInfoFromDescription(placemark.Description.Text);

                    if (waypointInfo != null)
                    {
                        this.tourEngine.AddWaypoint(waypointInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Generates a WaypointInfo object from given placemark description
        /// </summary>
        /// <param name="placemarkDescription">description text of placemark</param>
        /// <returns>waypoint info, or null when description couldn't be parsed</returns>
        private static WaypointInfo WaypointInfoFromDescription(string placemarkDescription)
        {
            var dict = ParseItemList(placemarkDescription);

            string waypointId = dict.GetValueOrDefault(KeyId, null);
            string description = dict.GetValueOrDefault(KeyDescription, null);

            if (waypointId != null)
            {
                return new WaypointInfo
                {
                    Id = waypointId,
                    Description = description,
                };
            }

            return null;
        }

        /// <summary>
        /// Loads all tracks from folder and adds them to the tour engine.
        /// </summary>
        /// <param name="trackFolder">folder to load from</param>
        private void LoadTracks(Folder trackFolder)
        {
            foreach (var element in trackFolder.Features)
            {
                if (element is Folder singleTrackFolder)
                {
                    // use description from track folder, not the track itself
                    var trackInfoTuple = this.TrackInfoFromDescription(singleTrackFolder.Description.Text);

                    var placemark = singleTrackFolder.Features.First(x => x is Placemark) as Placemark;

                    if (trackInfoTuple.Item1 != null)
                    {
                        trackInfoTuple.Item1.MapPointList = LoadTrackFromPlacemarkGeometry(placemark.Geometry);

                        this.tourEngine.AddTrack(trackInfoTuple.Item1);

                        if (trackInfoTuple.Item2 != null)
                        {
                            trackInfoTuple.Item2.MapPointList = new List<MapPoint>(trackInfoTuple.Item1.MapPointList);
                            trackInfoTuple.Item2.MapPointList.Reverse();

                            this.tourEngine.AddTrack(trackInfoTuple.Item2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a TrackInfo object, and possibly a reverse TrackInfo object, from given
        /// placemark description text.
        /// </summary>
        /// <param name="placemarkDescription">placemark description text to parse</param>
        /// <returns>tuple with track info and a reverse track info, which both may be null</returns>
        private Tuple<TrackInfo, TrackInfo> TrackInfoFromDescription(string placemarkDescription)
        {
            var dict = ParseItemList(placemarkDescription);

            string fromId = dict.GetValueOrDefault(KeyFrom, null);
            string toId = dict.GetValueOrDefault(KeyTo, null);
            string description = dict.GetValueOrDefault(KeyDescription, null);
            string reverseDescription = dict.GetValueOrDefault(KeyReverseDesc, null);

            string durationText = dict.GetValueOrDefault(KeyDuration, null);
            string reverseDurationText = dict.GetValueOrDefault(KeyReverseDuration, null);

            var source = this.tourEngine.FindOrCreateWaypointInfo(fromId);
            var target = this.tourEngine.FindOrCreateWaypointInfo(toId);

            var trackInfo = new TrackInfo
            {
                Source = source,
                Target = target,
                Description = description,
                Duration = ParseDurationText(durationText),
            };

            TrackInfo reverseTrackInfo = null;
            if (reverseDescription != null)
            {
                reverseTrackInfo = new TrackInfo
                {
                    Source = trackInfo.Target,
                    Target = trackInfo.Source,
                    Description = reverseDescription,
                    Duration = ParseDurationText(reverseDurationText),
                };
            }

            return new Tuple<TrackInfo, TrackInfo>(trackInfo, reverseTrackInfo);
        }

        /// <summary>
        /// Parses a duration text, e.g. "30m" or "2h" into a time span
        /// </summary>
        /// <param name="durationText">duration text to parse</param>
        /// <returns>time span</returns>
        /// <exception cref="FormatException">thrown duration text format wasn't correct</exception>
        private static System.TimeSpan ParseDurationText(string durationText)
        {
            if (durationText.EndsWith("m") &&
                int.TryParse(durationText.TrimEnd('m'), out int durationInMinutes))
            {
                return System.TimeSpan.FromMinutes(durationInMinutes);
            }
            else if (durationText.EndsWith("h") &&
                int.TryParse(durationText.TrimEnd('h'), out int durationInHours))
            {
                return System.TimeSpan.FromHours(durationInHours);
            }

            throw new FormatException($"invalid duration text: {durationText}");
        }

        /// <summary>
        /// Loads track points as lat/long coordinates list from placemark geometry, which may
        /// contain a LineString, a Track or a MultipleTrack object.
        /// </summary>
        /// <param name="geometry">geometry to load from</param>
        /// <returns>map points list</returns>
        private static List<MapPoint> LoadTrackFromPlacemarkGeometry(Geometry geometry)
        {
            var mapPointList = new List<MapPoint>();

            AddGeometryPointsToTrack(geometry, mapPointList);

            return mapPointList;
        }

        /// <summary>
        /// Adds geometry points to the map point list
        /// </summary>
        /// <param name="geometry">geometry to check</param>
        /// <param name="mapPointList">map point list</param>
        private static void AddGeometryPointsToTrack(Geometry geometry, List<MapPoint> mapPointList)
        {
            if (geometry is LineString lineString)
            {
                foreach (var vector in lineString.Coordinates)
                {
                    mapPointList.Add(new MapPoint(vector.Latitude, vector.Longitude, vector.Altitude));
                }
            }
            else if (geometry is SharpKml.Dom.GX.Track track)
            {
                AddGxTrackPointsToTrack(track, mapPointList);
            }
            else if (geometry is MultipleTrack multiTrack)
            {
                foreach (var gxtrack in multiTrack.Tracks)
                {
                    AddGxTrackPointsToTrack(gxtrack, mapPointList);
                }
            }
            else if (geometry is MultipleGeometry multiGeometry)
            {
                foreach (var subGeometry in multiGeometry.Geometry)
                {
                    AddGeometryPointsToTrack(subGeometry, mapPointList);
                }
            }
        }

        /// <summary>
        /// Adds all track points from a GX.Track to the map point list
        /// </summary>
        /// <param name="gxtrack">track to use</param>
        /// <param name="mapPointList">map point list</param>
        private static void AddGxTrackPointsToTrack(SharpKml.Dom.GX.Track gxtrack, List<MapPoint> mapPointList)
        {
            foreach (var vector in gxtrack.Coordinates)
            {
                mapPointList.Add(new MapPoint(vector.Latitude, vector.Longitude, vector.Altitude));
            }
        }

        /// <summary>
        /// Valid item titles in an item list
        /// </summary>
        private static readonly string[] ItemTitlesList = new string[]
        {
            KeyId,
            KeyDescription,
            KeyFrom,
            KeyTo,
            KeyReverseDesc,
            KeyDuration,
            KeyReverseDuration,
        };

        /// <summary>
        /// Parses an item list; a multi-line text where lines may start with an item title, e.g.
        /// "ID:" or "DESC:"; all items are parsed into a dictionary, with the values being the
        /// text after the item title. If a line starts without an item title, the text is added
        /// to the previous item.
        /// </summary>
        /// <param name="text">text to parse</param>
        /// <returns>mapping with item titles as key and item texts as values</returns>
        private static Dictionary<string, string> ParseItemList(string text)
        {
            var itemMapping = new Dictionary<string, string>();

            string[] lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string currentItemTitle = null;
            var currentItemText = new StringBuilder();

            foreach (string line in lines)
            {
                bool foundItemTitle = false;

                foreach (var itemTitle in ItemTitlesList)
                {
                    if (line.StartsWith(itemTitle + ":"))
                    {
                        if (currentItemTitle != null)
                        {
                            itemMapping[currentItemTitle] = currentItemText.ToString();
                        }

                        currentItemTitle = line.Substring(0, itemTitle.Length);

                        currentItemText.Clear();
                        currentItemText.Append(line.Substring(itemTitle.Length + 1).Trim());

                        foundItemTitle = true;
                        break;
                    }
                }

                if (!foundItemTitle &&
                    currentItemTitle != null)
                {
                    currentItemText.Append("\n");
                    currentItemText.Append(line.Trim());
                }
            }

            if (currentItemTitle != null)
            {
                itemMapping[currentItemTitle] = currentItemText.ToString();
            }

            return itemMapping;
        }
    }
}
