using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Dom.GX;
using SharpKml.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhereToFly.App.Logic;
using WhereToFly.App.Model;

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
        /// Creates a new KML data file from stream
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <param name="isKml">indicates if stream contains a kml or a kmz file</param>
        public KmlDataFile(Stream stream, bool isKml)
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
                var kmz = KmzFile.Open(stream);
                this.kml = kmz.GetDefaultKmlFile();
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
        /// Returns if the opened file contains any locations
        /// </summary>
        /// <returns>true when the file contains locations, false when not</returns>
        public bool HasLocations()
        {
            if (this.kml == null ||
                this.kml.Root == null)
            {
                return false;
            }

            foreach (var element in this.kml.Root.Flatten())
            {
                if (element is Placemark placemark &&
                    placemark.Geometry is Point point)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a list of tracks contained in the kml or kmz file.
        /// </summary>
        /// <returns>list of tracks found in the file</returns>
        public List<string> GetTrackList()
        {
            var trackList = new List<string>();

            if (this.kml == null ||
                this.kml.Root == null)
            {
                return trackList;
            }

            foreach (var element in this.kml.Root.Flatten())
            {
                if (element is Placemark placemark &&
                    placemark.Geometry is LineString linestring)
                {
                    trackList.Add(placemark.Name);
                }

                if (element is Placemark placemark2 &&
                    placemark2.Geometry is SharpKml.Dom.GX.Track track)
                {
                    trackList.Add(placemark2.Name);
                }

                if (element is Placemark placemark3 &&
                    placemark3.Geometry is MultipleTrack multiTrack)
                {
                    trackList.Add(placemark3.Name);
                }
            }

            return trackList;
        }

        /// <summary>
        /// Loads track from the kml or kmz file.
        /// </summary>
        /// <param name="trackIndex">index of track to load</param>
        /// <returns>loaded track</returns>
        public Track LoadTrack(int trackIndex)
        {
            int currentTrackIndex = 0;
            foreach (var element in this.kml.Root.Flatten())
            {
                if (element is Placemark placemark &&
                    placemark.Geometry is LineString lineString)
                {
                    if (trackIndex == currentTrackIndex)
                    {
                        return LoadTrackFromLineString(lineString);
                    }

                    currentTrackIndex++;
                }

                if (element is Placemark placemark2 &&
                    placemark2.Geometry is SharpKml.Dom.GX.Track track)
                {
                    if (trackIndex == currentTrackIndex)
                    {
                        return LoadTrackFromGXTrack(track);
                    }

                    currentTrackIndex++;
                }

                if (element is Placemark placemark3 &&
                    placemark3.Geometry is MultipleTrack multiTrack)
                {
                    if (trackIndex == currentTrackIndex)
                    {
                        return LoadTrackFromGXMultiTrack(multiTrack);
                    }

                    currentTrackIndex++;
                }
            }

            throw new FormatException("track number was not found in kml/kmz file");
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
        /// Creates empty Track object from given KML placemark object
        /// </summary>
        /// <param name="placemark">placemark object</param>
        /// <returns>newly created track object</returns>
        private static Track CreateTrackFromPlacemark(Placemark placemark)
        {
            return new Track
            {
                Id = placemark.Id ?? Guid.NewGuid().ToString("B"),
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
            var locationList = new List<Model.Location>();

            if (this.kml.Root == null)
            {
                return locationList;
            }

            foreach (var element in this.kml.Root.Flatten())
            {
                if (element is Placemark placemark &&
                    placemark.Geometry is Point point)
                {
                    locationList.Add(new Model.Location
                    {
                        Id = placemark.Id ?? Guid.NewGuid().ToString("B"),
                        Name = placemark.Name ?? "unknown",
                        Description = GetDescriptionFromPlacemark(placemark),
                        Type = MapPlacemarkToType(this.kml, placemark),
                        MapLocation = new MapPoint(point.Coordinate.Latitude, point.Coordinate.Longitude, point.Coordinate.Altitude)
                    });
                }
            }

            return locationList;
        }

        /// <summary>
        /// Returns sanitized description HTML text from given placemark.
        /// </summary>
        /// <param name="placemark">placemark to use</param>
        /// <returns>HTML description text</returns>
        private static string GetDescriptionFromPlacemark(Placemark placemark)
        {
            string text = placemark.Description?.Text ?? string.Empty;

            // no HTML tags? assume MarkDown
            if (!text.Contains("<") &&
                !text.Contains(">"))
            {
                text = HtmlConverter.FromMarkdown(text);
            }

            text = HtmlConverter.Sanitize(text);

            return text;
        }

        /// <summary>
        /// Mapping from a text that can occur in a placemark icon link, to a LocationType
        /// </summary>
        private static Dictionary<string, LocationType> iconLinkToLocationTypeMap = new Dictionary<string, LocationType>
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
                foreach (var iconLinkContentAndLocationType in iconLinkToLocationTypeMap)
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
                if (style != null &&
                    style is StyleMapCollection styleMap)
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
                    if (iconStyle != null &&
                        iconStyle is Style icon)
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
            var style = placemark.Styles.FirstOrDefault() as Style;
            if (style != null &&
                style.Icon != null &&
                style.Icon.Icon != null &&
                style.Icon.Icon.Href != null)
            {
                return style.Icon.Icon.Href.ToString();
            }

            return string.Empty;
        }
    }
}
