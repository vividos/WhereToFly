﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WhereToFly.Shared.Base;

namespace WhereToFly.App.Geo.Airspace
{
    /// <summary>
    /// Parses airspace files in the OpenAir text format. Format specification:
    /// http://www.winpilot.com/UsersGuide/UserAirspace.asp
    /// The parser does a least-effort try to parse airspaces. A new airspace starts when a new
    /// AC line appears or the file is at the end.
    /// The parser doesn't parse Terrain.txt files and ignores TO and TC elements. The parser also
    /// ignores the AT command (coordinate to place a label)
    /// </summary>
    public class OpenAirFileParser
    {
        /// <summary>
        /// Start of a comment line
        /// </summary>
        private const string CommentStart = "*";

        /// <summary>
        /// Stream to parse
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// List of all airspaces parsed
        /// </summary>
        private readonly List<Airspace> allAirspaces = new List<Airspace>();

        /// <summary>
        /// All parsing warnings or errors
        /// </summary>
        private readonly List<string> parsingErrors = new List<string>();

        /// <summary>
        /// Dictionary with current values of defined variables
        /// </summary>
        private readonly Dictionary<string, string> currentVariables = new Dictionary<string, string>();

        /// <summary>
        /// Currently parsed line
        /// </summary>
        private int currentLine;

        /// <summary>
        /// Currently parsed airspace; may be null
        /// </summary>
        private Airspace currentAirspace;

        /// <summary>
        /// Enumerable with all airspaces that were correctly parsed by this class
        /// </summary>
        public IEnumerable<Airspace> Airspaces => this.allAirspaces;

        /// <summary>
        /// All parsing error messages
        /// </summary>
        public IEnumerable<string> ParsingErrors => this.parsingErrors;

        /// <summary>
        /// Creates a new OpenAir file parses
        /// </summary>
        /// <param name="stream">stream to parse</param>
        public OpenAirFileParser(Stream stream)
        {
            this.stream = stream;

            this.Parse();
        }

        /// <summary>
        /// Parses the stream containing airspaces in OpenAir text format
        /// </summary>
        private void Parse()
        {
            this.currentLine = 0;
            using (var reader = new StreamReader(this.stream))
            {
                do
                {
                    string line = reader.ReadLine().Trim();
                    this.currentLine++;

                    int posComment = line.IndexOf(CommentStart);
                    if (posComment != -1)
                    {
                        line = line.Substring(0, posComment).Trim();
                    }

                    if (line.Length == 0)
                    {
                        continue;
                    }

                    this.ParseLine(line);
                }
                while (!reader.EndOfStream);
            }

            this.StoreCurrentAirspace();
        }

        /// <summary>
        /// Stores the current airspace in the list when there is one
        /// </summary>
        private void StoreCurrentAirspace()
        {
            if (this.currentAirspace != null)
            {
                if (this.currentAirspace.Name == null)
                {
                    this.AddParsingError($"airspace with class {this.currentAirspace.Class} has no name");
                    this.currentAirspace.Name = "???";
                }

                if (this.currentAirspace.Floor == null)
                {
                    this.AddParsingError($"airspace {this.currentAirspace.Name} has no floor, setting GND");
                    this.currentAirspace.Floor = new Altitude(AltitudeType.GND);
                }

                if (this.currentAirspace.Ceiling == null)
                {
                    this.AddParsingError($"airspace {this.currentAirspace.Name} has no ceiling, setting UNLIMITED");
                    this.currentAirspace.Ceiling = new Altitude(AltitudeType.Unlimited);
                }

                if (this.currentAirspace.Geometry == null)
                {
                    this.AddParsingError($"airspace {this.currentAirspace.Name} has no geometry!");
                }

                this.allAirspaces.Add(this.currentAirspace);
            }

            // Note that only variable D is removed, or else some airspaces with use common center
            // coordinates don't work anymore.
            this.currentVariables.Remove("D");
        }

        /// <summary>
        /// Adds a new parsing error text
        /// </summary>
        /// <param name="errorText">error text to add</param>
        private void AddParsingError(string errorText)
        {
            string text = $"error on line {this.currentLine}: {errorText}";

            Debug.WriteLine(text);
            this.parsingErrors.Add(text);
        }

        /// <summary>
        /// Parses a single line
        /// </summary>
        /// <param name="line">line from OpenAir file</param>
        private void ParseLine(string line)
        {
            int posFirstSpace = line.IndexOf(' ');
            if (posFirstSpace == -1)
            {
                this.AddParsingError("invalid OpenAir line: " + line);
                return;
            }

            string command = line.Substring(0, posFirstSpace);

            this.ParseCommand(command, line.Substring(posFirstSpace + 1));
        }

        /// <summary>
        /// Parses a command from a single line
        /// </summary>
        /// <param name="command">two-letter command</param>
        /// <param name="data">additional data from the line</param>
        private void ParseCommand(string command, string data)
        {
            switch (command)
            {
                case "AC":
                    this.StoreCurrentAirspace();
                    var airspaceClass = ParseAirspaceClass(data);
                    this.currentAirspace = new Airspace(airspaceClass);
                    break;

                case "AN":
                    this.ParseAN(data);
                    break;

                case "AL":
                case "AH":
                    this.ParseALAH(command, data);
                    break;

                case "AY": // Naviter extension
                    this.currentAirspace.AirspaceType = data;
                    break;

                case "AF": // Naviter extension
                    this.currentAirspace.Frequency = data;
                    break;

                case "AG": // Naviter extension
                    this.currentAirspace.CallSign = data;
                    break;

                case "SP":
                case "SB":
                    this.ParseSPSB(command, data);
                    break;

                case "DP":
                    this.ParseDP(data);
                    break;

                case "V":
                    this.ParseV(data);
                    break;

                case "DC":
                    this.ParseDC(data);
                    break;

                case "DA":
                    this.ParseDA(data);
                    break;

                case "DB":
                    this.ParseDB(data);
                    break;

                case "AT":
                case "TO":
                case "TC":
                    // ignore these commands and output no warning
                    break;

                default:
                    this.AddParsingError($"unrecognized command: {command} {data}");
                    break;
            }
        }

        /// <summary>
        /// Mapping from OpenAir airspace class as text to AirspaceClass enum
        /// </summary>
        private static readonly Dictionary<string, AirspaceClass> AirspaceMapping = new Dictionary<string, AirspaceClass>
        {
            { "R", AirspaceClass.Restricted },
            { "Q", AirspaceClass.Danger },
            { "P", AirspaceClass.Prohibited },
            { "A", AirspaceClass.A },
            { "B", AirspaceClass.B },
            { "C", AirspaceClass.C },
            { "D", AirspaceClass.D },
            { "E", AirspaceClass.E },
            { "F", AirspaceClass.F },
            { "G", AirspaceClass.G },
            { "GP", AirspaceClass.GliderProhibited },
            { "CTR", AirspaceClass.CTR },
            { "W", AirspaceClass.WaveWindow },
            { "RMZ", AirspaceClass.RMZ },
            { "TMZ", AirspaceClass.TMZ },
        };

        /// <summary>
        /// Parses airspace class from AC command
        /// </summary>
        /// <param name="airspaceClassText">airspace class as text</param>
        /// <returns>parsed airspace class, or Unknown</returns>
        private static AirspaceClass ParseAirspaceClass(string airspaceClassText)
        {
            return AirspaceMapping.GetValueOrDefault(airspaceClassText, AirspaceClass.Unknown);
        }

        /// <summary>
        /// Parses AN line (airspace name)
        /// </summary>
        /// <param name="data">data text</param>
        private void ParseAN(string data)
        {
            if (this.currentAirspace == null)
            {
                this.AddParsingError("Warning: Airspace name (AN) before AC command; ignoring!");
            }
            else
            {
                if (string.IsNullOrEmpty(this.currentAirspace.Name))
                {
                    this.currentAirspace.Name = data;
                }
                else
                {
                    this.AddParsingError("already set Airspace name (AN) from previous entry");
                    this.currentAirspace.Name += data;
                }
            }
        }

        /// <summary>
        /// Parse AL and AH line
        /// </summary>
        /// <param name="command">command to parse</param>
        /// <param name="data">command data to parse</param>
        private void ParseALAH(string command, string data)
        {
            bool isFloor = command == "AL";

            if (this.currentAirspace != null)
            {
                var altitude = this.ParseAltitude(data);
                if (isFloor)
                {
                    this.currentAirspace.Floor = altitude;
                }
                else
                {
                    this.currentAirspace.Ceiling = altitude;
                }
            }
            else
            {
                this.AddParsingError($"command {command} without a current airspace");
            }
        }

        /// <summary>
        /// Parse altitude. Many variations are available; try to parse most common cases. As last
        /// resort, return a Textual altitude object that must be interpreted by a human.
        /// </summary>
        /// <param name="data">altitude text to parse</param>
        /// <returns>altitude object</returns>
        private Altitude ParseAltitude(string data)
        {
            string localData = data.ToUpperInvariant();

            // parse fixed strings
            switch (localData)
            {
                case "SFC":
                case "GND":
                case "0FT GND":
                    return new Altitude(AltitudeType.GND);
                case "UNL":
                case "UNLTD":
                case "UNLIM":
                case "UNLIMITED":
                    return new Altitude(AltitudeType.Unlimited);
                default:
                    break;
            }

            // parse flight level
            if (localData.StartsWith("FL"))
            {
                if (TryParseNumber(
                    localData.Substring(2).Trim(),
                    out double flightLevel))
                {
                    return new Altitude(flightLevel, AltitudeType.FlightLevel);
                }
                else
                {
                    this.AddParsingError("invalid flight level altitude: " + data);
                }
            }

            // parse fixed heights
            AltitudeType type = AltitudeType.Textual;
            if (localData.EndsWith("MSL") ||
                localData.EndsWith("AMSL") ||
                localData.EndsWith("M") ||
                localData.EndsWith("FT") ||
                localData.All(ch => char.IsDigit(ch)))
            {
                type = AltitudeType.AMSL;
                localData = localData.Replace("AMSL", string.Empty)
                    .Replace("MSL", string.Empty).Trim();
            }

            if (localData.EndsWith("AGL") ||
                localData.EndsWith("GND"))
            {
                type = AltitudeType.AGL;
                localData = localData.Replace("AGL", string.Empty)
                    .Replace("GND", string.Empty).Trim();
            }

            if (type != AltitudeType.Textual)
            {
                bool unitFeet = true; // when empty, use feet

                if (localData.EndsWith("M"))
                {
                    localData = localData.Replace("M", string.Empty).Trim();
                    unitFeet = false;
                }

                if (localData.EndsWith("FT"))
                {
                    localData = localData.Replace("FT", string.Empty).Trim();
                }

                if (TryParseNumber(localData, out double value))
                {
                    if (!unitFeet)
                    {
                        value /= Spatial.Constants.FactorFeetToMeter;
                    }

                    return new Altitude(value, type);
                }
            }

            return new Altitude(data);
        }

        /// <summary>
        /// Parses V command, defining a variable's value. Example:
        /// <code>V X = 39:29.9 N 119:46.1 W</code>
        /// </summary>
        /// <param name="data">data to parse</param>
        private void ParseV(string data)
        {
            int posEqual = data.IndexOf('=');
            if (posEqual == -1)
            {
                this.AddParsingError("invalid variable definition: V " + data);
                return;
            }

            string key = data.Substring(0, posEqual).Trim().ToUpperInvariant();
            string value = data.Substring(posEqual + 1).Trim();
            this.currentVariables[key] = value;
        }

        /// <summary>
        /// Parses SP (select pen) and SB (select brush) commands. Examples:
        /// </summary>
        /// <param name="command">command to parse</param>
        /// <param name="data">data to parse</param>
        private void ParseSPSB(string command, string data)
        {
            if (this.currentAirspace == null)
            {
                this.AddParsingError($"invalid {command} command without an open airspace");
                return;
            }

            string[] parts = data.Split(',');

            if ((command == "SP" && parts.Length != 5) ||
                (command == "SB" && parts.Length != 3))
            {
                this.AddParsingError($"invalid {command} command data: {data}");
                return;
            }

            if (command == "SP")
            {
                parts = parts.Skip(2).ToArray();
            }

            int red = Convert.ToInt32(parts[0]);
            int green = Convert.ToInt32(parts[1]);
            int blue = Convert.ToInt32(parts[2]);

            this.currentAirspace.Color = $"{red:X2}{green:X2}{blue:X2}";
        }

        /// <summary>
        /// Parses DP (polygon point) command. Example:
        /// DP 52:23:00 N 005:50:00 E
        /// </summary>
        /// <param name="data">polygon point to parse</param>
        private void ParseDP(string data)
        {
            var coord = this.ParseCoord(data);

            this.AddPolygonSegment(new Polygon.PolygonPoint
            {
                Point = coord
            });
        }

        /// <summary>
        /// Parses circle definition. Example:
        /// DC 5
        /// </summary>
        /// <param name="data">circle radius, in nautical miles</param>
        private void ParseDC(string data)
        {
            var x = this.currentVariables.GetValueOrDefault("X", null);
            Coord centerCoord = this.ParseCoord(x);

            if (centerCoord == null)
            {
                this.AddParsingError("invalid center coordinates: " + x ?? "undefined");
                return;
            }

            if (!TryParseNumber(
                data,
                out double radiusNauticalMiles))
            {
                this.AddParsingError("error parsing circle radius: " + data);
                return;
            }

            double radiusInMeter = radiusNauticalMiles * Spatial.Constants.FactorNauticalMilesToMeter;

            if (this.currentAirspace.Geometry != null)
            {
                string geometryType = this.currentAirspace.Geometry.GetType().Name;
                this.AddParsingError($"another geometry {geometryType} is already set for airspace; ignoring circle geometry");
                return;
            }

            this.currentAirspace.Geometry = new Circle
            {
                Center = centerCoord,
                Radius = radiusInMeter
            };
        }

        /// <summary>
        /// Parses arc segment definition. Example:
        /// DA 10,270,290
        /// </summary>
        /// <param name="data">data to parse</param>
        private void ParseDA(string data)
        {
            var x = this.currentVariables.GetValueOrDefault("X", null);
            Coord centerCoord = this.ParseCoord(x);

            if (centerCoord == null)
            {
                this.AddParsingError("invalid center coordinates: " + x ?? "undefined");
                return;
            }

            string[] elements = data.Split(',');

            if (elements.Length != 3)
            {
                this.AddParsingError("invalid number of elements in DA command: " + data);
                return;
            }

            if (!TryParseNumber(
                elements[0],
                out double radiusNauticalMiles))
            {
                this.AddParsingError("error parsing arc radius: " + data);
                return;
            }

            double radiusInMeter = radiusNauticalMiles * Spatial.Constants.FactorNauticalMilesToMeter;

            this.AddPolygonSegment(new Polygon.PolygonArcSegment
            {
                Center = centerCoord,
                Radius = radiusInMeter,
                StartAngle = Convert.ToDouble(elements[1]),
                EndAngle = Convert.ToDouble(elements[2]),
                Direction = this.GetDirectionFromCurrentVariable(),
            });
        }

        /// <summary>
        /// Parses arc definition. Example:
        /// DB 52:29:23 N 7:17:50 E,52:26:00 N 7:03:30 E
        /// </summary>
        /// <param name="data">data to parse</param>
        private void ParseDB(string data)
        {
            var x = this.currentVariables.GetValueOrDefault("X", null);
            Coord centerCoord = this.ParseCoord(x);

            if (centerCoord == null)
            {
                this.AddParsingError("invalid center coordinates: " + x ?? "undefined");
                return;
            }

            string[] elements = data.Split(',');
            if (elements.Length != 2)
            {
                this.AddParsingError("invalid number of elements in DB command: " + data);
                return;
            }

            var startCoord = this.ParseCoord(elements[0]);
            var endCoord = this.ParseCoord(elements[1]);

            if (startCoord == null || endCoord == null)
            {
                this.AddParsingError("invalid coordinates in DB command: " + data);
                return;
            }

            this.AddPolygonSegment(new Polygon.PolygonArc
            {
                Center = centerCoord,
                Start = startCoord,
                End = endCoord,
                Direction = this.GetDirectionFromCurrentVariable(),
            });
        }

        /// <summary>
        /// Adds a polygon segment to the current airspace; when no geometry is active, creates a
        /// new polygon geometry.
        /// </summary>
        /// <param name="polygonSegment">polygon segment to add</param>
        private void AddPolygonSegment(Polygon.PolygonSegment polygonSegment)
        {
            if (this.currentAirspace == null)
            {
                this.AddParsingError("adding polygon segment without starting airspace");
                return;
            }

            if (this.currentAirspace.Geometry == null)
            {
                this.currentAirspace.Geometry = new Polygon();
            }
            else
            {
                if (this.currentAirspace.Geometry is Circle)
                {
                    this.AddParsingError("current airspace already has a Circle geometry");
                    return;
                }
            }

            var polygon = this.currentAirspace.Geometry as Polygon;
            polygon.Segments.Add(polygonSegment);
        }

        /// <summary>
        /// Returns arc direction from current D variable
        /// </summary>
        /// <returns>arc direction</returns>
        private Polygon.ArcDirection GetDirectionFromCurrentVariable()
        {
            string direction = this.currentVariables.GetValueOrDefault("D", "+");
            if (direction != "+" && direction != "-")
            {
                this.AddParsingError("invalid direction variable value: " + direction);
                direction = "+";
            }

            return direction == "+" ? Polygon.ArcDirection.Clockwise : Polygon.ArcDirection.CounterClockwise;
        }

        /// <summary>
        /// Array of coordinate separator characters
        /// </summary>
        private static char[] coordinateSeparators = new char[] { 'N', 'S', 'E', 'W' };

        /// <summary>
        /// Parses coordinates, various formats. Examples:
        /// 52:23:00 N 005:50:00 E
        /// 52:21.30 S 005:52.30 W
        /// 52:21:30.123 S 005:52:30.456 W
        /// </summary>
        /// <param name="coordinates">coordinates text</param>
        /// <returns>coordinates object, or null when there was a format error</returns>
        private Coord ParseCoord(string coordinates)
        {
            if (string.IsNullOrWhiteSpace(coordinates))
            {
                return null;
            }

            int posSeparator = coordinates.IndexOf('N');
            if (posSeparator == -1)
            {
                posSeparator = coordinates.IndexOf('S');
            }

            if (posSeparator == -1)
            {
                this.AddParsingError("invalid coordinates format: " + coordinates);
                return null;
            }

            string latitudeText = coordinates.Substring(0, posSeparator + 1);
            string longitudeText = coordinates.Substring(posSeparator + 1).Trim();

            if (!TryParseLatLong(latitudeText, out double latitude) ||
                !TryParseLatLong(longitudeText, out double longitude))
            {
                this.AddParsingError("invalid coordinates format: " + coordinates);
                return null;
            }

            return new Coord
            {
                Latitude = latitude,
                Longitude = longitude,
            };
        }

        /// <summary>
        /// Parses latitude and longitude text values
        /// </summary>
        /// <param name="latLongText">latiude or longitude as text</param>
        /// <param name="latLong">parsed latitude/longitude value</param>
        /// <returns>true when parsing was successful, false when not</returns>
        private static bool TryParseLatLong(string latLongText, out double latLong)
        {
            latLong = 0.0;

            bool isNegative = false;

            if (latLongText.EndsWith("S") ||
                latLongText.EndsWith("W"))
            {
                isNegative = true;
            }

            latLongText = latLongText.TrimEnd(coordinateSeparators).TrimEnd();

            string[] parts = latLongText.Split(':');
            if (parts.Length != 2 && parts.Length != 3)
            {
                return false;
            }

            if (parts.Length == 2 &&
                TryParseNumber(parts[0], out double degrees) &&
                TryParseNumber(parts[1], out double minutes))
            {
                latLong = degrees + (minutes / 60.0);
                if (isNegative)
                {
                    latLong *= -1;
                }

                return true;
            }

            if (parts.Length == 3 &&
                TryParseNumber(parts[0], out double degrees2) &&
                TryParseNumber(parts[1], out double minutes2) &&
                TryParseNumber(parts[2], out double seconds2))
            {
                latLong = degrees2 + (minutes2 / 60.0) + (seconds2 / 3600.0);
                if (isNegative)
                {
                    latLong *= -1;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries parsing a number that can have fractional digits, separated with a dot as
        /// separator.
        /// </summary>
        /// <param name="text">text to parse</param>
        /// <param name="value">parsed value</param>
        /// <returns>true when successful, false when not</returns>
        private static bool TryParseNumber(string text, out double value)
        {
            var formatProvider = System.Globalization.CultureInfo.InvariantCulture;

            return double.TryParse(text, System.Globalization.NumberStyles.Float, formatProvider, out value);
        }
    }
}