using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Dom.GX;
using SharpKml.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WhereToFly.App.Geo.Spatial;
using WhereToFly.App.Logic;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Geo.DataFormats
{
    /// <summary>
    /// Geo data file for .kml and .kmz files; uses the SharpKml library.
    /// </summary>
    internal class KmlDataFile : IGeoDataFile
    {
        /// <summary>
        /// Opened KML file
        /// </summary>
        private readonly KmlFile kml;

        /// <summary>
        /// Indicates if KML file contains a track from an XC Tracer device
        /// </summary>
        private readonly bool hasXCTracerTrack;

        /// <summary>
        /// List of track placemarks
        /// </summary>
        private readonly List<Placemark> trackPlacemarkList = new List<Placemark>();

        /// <summary>
        /// List of location placemarks
        /// </summary>
        private readonly List<Placemark> locationPlacemarkList = new List<Placemark>();

        /// <summary>
        /// Creates a new KML data file from stream
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <param name="filename">filename of kml or kmz file to load</param>
        /// <param name="isKml">indicates if stream contains a kml or a kmz file</param>
        public KmlDataFile(Stream stream, string filename, bool isKml)
        {
            if (isKml)
            {
                using (var reader = new StreamReader(stream))
                {
                    this.kml = KmlFile.Load(reader);

                    if (this.kml.Root == null &&
                        stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.Begin);

                        this.kml = this.ReadLegacyKmlStream(stream, isKml);
                    }
                }
            }
            else
            {
                using (var kmz = KmzFile.Open(stream))
                {
                    this.kml = kmz.GetDefaultKmlFile();
                }
            }

            this.hasXCTracerTrack = filename.Contains("-XTR-");

            if (this.kml != null)
            {
                this.ScanKml();
            }
        }

        /// <summary>
        /// Tries to read a legacy kml stream by using SharpKml Parser class directly.
        /// </summary>
        /// <param name="stream">stream to read</param>
        /// <param name="isKml">indicates if stream contains a kml or a kmz file</param>
        /// <returns>parsed kml file, or null when file couldn't be loaded</returns>
        private KmlFile ReadLegacyKmlStream(Stream stream, bool isKml)
        {
            if (isKml)
            {
                var parser = new Parser();
                using (var reader = new StreamReader(stream))
                {
                    string xml = reader.ReadToEnd();
                    parser.ParseString(xml, false); // ignore the namespaces
                }

                return KmlFile.Create(parser.Root, true);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Scans loaded KmlFile for track and location placemarks.
        /// </summary>
        private void ScanKml()
        {
            if (this.kml?.Root == null)
            {
                return;
            }

            foreach (var element in this.kml.Root.Flatten())
            {
                if (element is Placemark placemark)
                {
                    if (placemark.Geometry is LineString ||
                        placemark.Geometry is SharpKml.Dom.GX.Track ||
                        placemark.Geometry is MultipleTrack ||
                        placemark.Geometry is MultipleGeometry)
                    {
                        this.trackPlacemarkList.Add(placemark);
                    }

                    if (placemark.Geometry is Point)
                    {
                        this.locationPlacemarkList.Add(placemark);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if the opened file contains any locations
        /// </summary>
        /// <returns>true when the file contains locations, false when not</returns>
        public bool HasLocations() => this.locationPlacemarkList.Any();

        /// <summary>
        /// Returns a list of tracks contained in the kml or kmz file.
        /// </summary>
        /// <returns>list of tracks found in the file</returns>
        public List<string> GetTrackList()
        {
            return
                (from trackPlacemark in this.trackPlacemarkList
                 select trackPlacemark.Name).ToList();
        }

        /// <summary>
        /// Loads track from the kml or kmz file.
        /// </summary>
        /// <param name="trackIndex">index of track to load</param>
        /// <returns>loaded track</returns>
        public Track LoadTrack(int trackIndex)
        {
            if (trackIndex >= this.trackPlacemarkList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(trackIndex));
            }

            var placemark = this.trackPlacemarkList[trackIndex];

            Track track = null;

            if (placemark.Geometry is LineString lineString)
            {
                track = LoadTrackFromLineString(lineString);
            }
            else if (placemark.Geometry is SharpKml.Dom.GX.Track gxtrack)
            {
                track = LoadTrackFromGXTrack(gxtrack);
            }
            else if (placemark.Geometry is MultipleTrack multiTrack)
            {
                track = LoadTrackFromGXMultiTrack(multiTrack);
            }
            else if (placemark.Geometry is MultipleGeometry multiGeometry)
            {
                track = this.LoadTrackFromMultipleGeometry(multiGeometry);
            }
            else
            {
                throw new FormatException("track number was not found in kml/kmz file");
            }

            if (this.hasXCTracerTrack)
            {
                track.IsFlightTrack = true;
                if (!track.TrackPoints.Any(trackPoint => trackPoint.Time.HasValue))
                {
                    track.CalcXCTracerTimePoints();
                }
            }

            return track;
        }

        /// <summary>
        /// Loads track from LineString object
        /// </summary>
        /// <param name="lineString">line string object</param>
        /// <returns>loaded track</returns>
        private static Track LoadTrackFromLineString(LineString lineString)
        {
            var track = CreateTrackFromPlacemark(lineString.Parent as Placemark);

            foreach (var vector in lineString.Coordinates)
            {
                TrackPoint trackPoint = GetTrackPointFromVector(vector);

                track.TrackPoints.Add(trackPoint);
            }

            return track;
        }

        /// <summary>
        /// Creates track point object from SharpKml Vector object
        /// </summary>
        /// <param name="vector">vector object</param>
        /// <returns>track point object</returns>
        private static TrackPoint GetTrackPointFromVector(SharpKml.Base.Vector vector)
        {
            double? altitude = null;
            if (vector.Altitude.HasValue)
            {
                altitude = vector.Altitude;
            }

            return new TrackPoint(vector.Latitude, vector.Longitude, altitude, heading: null);
        }

        /// <summary>
        /// Loads track from given GX.Track object.
        /// </summary>
        /// <param name="gxtrack">track object</param>
        /// <returns>loaded track </returns>
        private static Track LoadTrackFromGXTrack(SharpKml.Dom.GX.Track gxtrack)
        {
            var track = CreateTrackFromPlacemark(gxtrack.Parent as Placemark);

            AddGxTrackPointsToTrack(track, gxtrack);

            return track;
        }

        /// <summary>
        /// Loads track from given GX.MultiTrack object, containing one or several GX.Track
        /// objects.
        /// </summary>
        /// <param name="multiTrack">multi track object</param>
        /// <returns>loaded track </returns>
        private static Track LoadTrackFromGXMultiTrack(MultipleTrack multiTrack)
        {
            var track = CreateTrackFromPlacemark(multiTrack.Parent as Placemark);

            foreach (var gxtrack in multiTrack.Tracks)
            {
                AddGxTrackPointsToTrack(track, gxtrack);
            }

            return track;
        }

        /// <summary>
        /// Loads track from given MultiGeometry object, containing multiple geometry objects.
        /// </summary>
        /// <param name="multiGeometry">multi geometry object</param>
        /// <returns>loaded track </returns>
        private Track LoadTrackFromMultipleGeometry(MultipleGeometry multiGeometry)
        {
            var track = CreateTrackFromPlacemark(multiGeometry.Parent as Placemark);

            foreach (var geometry in multiGeometry.Geometry)
            {
                if (geometry is LineString lineString)
                {
                    foreach (var vector in lineString.Coordinates)
                    {
                        TrackPoint trackPoint = GetTrackPointFromVector(vector);

                        track.TrackPoints.Add(trackPoint);
                    }
                }
                else if (geometry is SharpKml.Dom.GX.Track gxtrack)
                {
                    AddGxTrackPointsToTrack(track, gxtrack);
                }
                else if (geometry is MultipleTrack multiTrack)
                {
                    foreach (var gxtrack2 in multiTrack.Tracks)
                    {
                        AddGxTrackPointsToTrack(track, gxtrack2);
                    }
                }
                else
                {
                    Debug.WriteLine($"unhandled geometry type: {geometry.GetType().FullName}");
                }
            }

            return track;
        }

        /// <summary>
        /// Creates empty Track object from given KML placemark object
        /// </summary>
        /// <param name="placemark">placemark object</param>
        /// <returns>newly created track object</returns>
        private static Track CreateTrackFromPlacemark(Placemark placemark)
        {
            return new Track
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = placemark.Name ?? "Track",
            };
        }

        /// <summary>
        /// Adds GX.Track points to given Track object
        /// </summary>
        /// <param name="track">track object to add points to</param>
        /// <param name="gxtrack">GX.Track object to use</param>
        private static void AddGxTrackPointsToTrack(Track track, SharpKml.Dom.GX.Track gxtrack)
        {
            foreach (var vector in gxtrack.Coordinates)
            {
                TrackPoint trackPoint = GetTrackPointFromVector(vector);

                track.TrackPoints.Add(trackPoint);
            }
        }

        /// <summary>
        /// Loads location list from kml or kmz file
        /// </summary>
        /// <returns>list of locations found in the file</returns>
        public List<Model.Location> LoadLocationList()
        {
            return
                (from placemark in this.locationPlacemarkList
                 let point = placemark.Geometry as Point
                 select new Model.Location
                 {
                     Id = placemark.Id ?? Guid.NewGuid().ToString("B"),
                     Name = placemark.Name ?? "unknown",
                     Description = GetDescriptionFromPlacemark(placemark),
                     Type = MapPlacemarkToType(this.kml, placemark),
                     MapLocation = new MapPoint(point.Coordinate.Latitude, point.Coordinate.Longitude, point.Coordinate.Altitude)
                 }).ToList();
        }

        /// <summary>
        /// Returns sanitized description HTML text from given placemark.
        /// </summary>
        /// <param name="placemark">placemark to use</param>
        /// <returns>HTML description text</returns>
        private static string GetDescriptionFromPlacemark(Placemark placemark)
        {
            string text = placemark.Description?.Text ?? string.Empty;

            text = HtmlConverter.Sanitize(text);
            text = HtmlConverter.FromHtmlOrMarkdown(text);

            return text;
        }

        /// <summary>
        /// Mapping from a text that can occur in a placemark icon link, to a LocationType
        /// </summary>
        private static readonly Dictionary<string, LocationType> IconLinkToLocationTypeMap = new Dictionary<string, LocationType>
        {
            // paraglidingsports.com types
            { "iconpg_sp.png", LocationType.FlyingTakeoff },
            { "iconpg_spk.png", LocationType.FlyingTakeoff },
            { "iconpg_spw.png", LocationType.FlyingWinchTowing },
            { "iconpg_lp.png", LocationType.FlyingLandingPlace },

            // DHV Geländedatenbank
            { "windsack_rot", LocationType.FlyingTakeoff },
            { "windsack_gruen", LocationType.FlyingLandingPlace },
            { "windsack_blau", LocationType.FlyingWinchTowing },

            // general Google Maps types
            { "parking", LocationType.Parking },
            { "dining", LocationType.Restaurant },
            { "bus", LocationType.PublicTransportBus },
            { "rail", LocationType.PublicTransportTrain },

            // special cases
            { "red-bull-x-alps.appspot.com/tp", LocationType.Turnpoint },
        };

        /// <summary>
        /// Maps a placemark to a location type
        /// </summary>
        /// <param name="kml">kml file where the placemark is in</param>
        /// <param name="placemark">placemark to use</param>
        /// <returns>location type</returns>
        private static LocationType MapPlacemarkToType(KmlFile kml, Placemark placemark)
        {
            string iconLink = GetPlacemarkStyleIcon(kml, placemark);

            if (!string.IsNullOrWhiteSpace(iconLink))
            {
                foreach (var iconLinkContentAndLocationType in IconLinkToLocationTypeMap)
                {
                    if (iconLink.Contains(iconLinkContentAndLocationType.Key))
                    {
                        return iconLinkContentAndLocationType.Value;
                    }
                }
            }

            var name = placemark.Name ?? string.Empty;

            if (name.StartsWith("SP ") ||
                name.StartsWith("SP-HG "))
            {
                return LocationType.FlyingTakeoff;
            }

            if (name.StartsWith("LP "))
            {
                return LocationType.FlyingLandingPlace;
            }

            if (name.StartsWith("P "))
            {
                return LocationType.Parking;
            }

            if (name.StartsWith("TP") ||
                name.StartsWith("XCP"))
            {
                return LocationType.Turnpoint;
            }

            return LocationType.Waypoint;
        }

        /// <summary>
        /// Retrieves style icon from a placemark
        /// </summary>
        /// <param name="kml">kml file where the placemark is in</param>
        /// <param name="placemark">placemark to check</param>
        /// <returns>style URL, or empty string when style couldn't be retrieved</returns>
        private static string GetPlacemarkStyleIcon(KmlFile kml, Placemark placemark)
        {
            try
            {
                if (placemark.StyleUrl != null)
                {
                    return GetStyleIconFromStyleUrl(kml, placemark);
                }
                else if (placemark.Styles.Any())
                {
                    return GetStyleIconFromStyleCollection(placemark);
                }
            }
            catch (Exception)
            {
                // ignore any errors while trying to parse
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets style icon from StyleUrl property of placemark
        /// </summary>
        /// <param name="kml">kml file where the placemark is in</param>
        /// <param name="placemark">placemark to check</param>
        /// <returns>style URL, or empty string when style couldn't be retrieved</returns>
        private static string GetStyleIconFromStyleUrl(KmlFile kml, Placemark placemark)
        {
            string styleUrl = placemark.StyleUrl.ToString();

            if (styleUrl.StartsWith("#"))
            {
                var style = kml.FindStyle(styleUrl.Substring(1));
                if (style is StyleMapCollection styleMap)
                {
                    string link = GetStyleMapNormalStyleIconLink(kml, styleMap);
                    if (!string.IsNullOrEmpty(link))
                    {
                        return link;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Retrieves link for "normal" style icon, from given style map
        /// </summary>
        /// <param name="kml">kml file where the style map is in</param>
        /// <param name="styleMap">style map to search</param>
        /// <returns>style URL, or null when style couldn't be retrieved</returns>
        private static string GetStyleMapNormalStyleIconLink(KmlFile kml, StyleMapCollection styleMap)
        {
            var normalStyle = styleMap.First(x => x.State.HasValue && x.State.Value == StyleState.Normal);
            if (normalStyle != null)
            {
                string normalStyleUrl = normalStyle.StyleUrl.ToString();
                if (normalStyleUrl.StartsWith("#"))
                {
                    var iconStyle = kml.FindStyle(normalStyleUrl.Substring(1));
                    if (iconStyle is Style icon)
                    {
                        return icon.Icon.Icon.Href.ToString();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets style icon from Styles collection property of placemark
        /// </summary>
        /// <param name="placemark">placemark to check</param>
        /// <returns>style URL, or empty string when style couldn't be retrieved</returns>
        private static string GetStyleIconFromStyleCollection(Placemark placemark)
        {
            if (placemark.Styles.FirstOrDefault() is Style style &&
                style?.Icon?.Icon?.Href != null)
            {
                return style.Icon.Icon.Href.ToString();
            }

            return string.Empty;
        }
    }
}
