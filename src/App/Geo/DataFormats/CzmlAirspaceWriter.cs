using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using WhereToFly.App.Geo.Airspace;
using WhereToFly.Shared.Geo;

namespace WhereToFly.App.Geo.DataFormats
{
    /// <summary>
    /// CZML Writer for airspaces
    /// </summary>
    public static class CzmlAirspaceWriter
    {
        /// <summary>
        /// Value to use when an Unlimited Altitude object should be displayed; in meter
        /// </summary>
        private static readonly double UnlimitedAltitudeHeightInMeter = 10000.0;

        /// <summary>
        /// Default airspace color
        /// </summary>
        private static readonly string DefaultAirspaceColor = "C0C0C0";

        /// <summary>
        /// CZML packet header
        /// </summary>
        public class CzmlPacketHeader
        {
            /// <summary>
            /// Document ID
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; } = "document";

            /// <summary>
            /// Packet name
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Packet description; may be null
            /// </summary>
            [JsonProperty("description")]
            public string Description { get; }

            /// <summary>
            /// Packet version
            /// </summary>
            [JsonProperty("version")]
            public string Version { get; set; } = "1.0";

            /// <summary>
            /// Creates a new packet header
            /// </summary>
            /// <param name="name">name of document</param>
            /// <param name="description">description for packet</param>
            public CzmlPacketHeader(string name, string description)
            {
                this.Name = name;
                this.Description = description;
            }
        }

        /// <summary>
        /// CZML object, containing various entity objects
        /// </summary>
        public class CzmlObject
        {
            /// <summary>
            /// ID of object
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; } = Guid.NewGuid().ToString("B");

            /// <summary>
            /// Object name
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Object description
            /// </summary>
            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }

            /// <summary>
            /// Position; used by Point, Cylinder, etc.
            /// </summary>
            [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
            public Position Position { get; set; }

            /// <summary>
            /// Cylinder entity
            /// </summary>
            [JsonProperty("cylinder", NullValueHandling = NullValueHandling.Ignore)]
            public Cylinder Cylinder { get; set; }

            /// <summary>
            /// Polygon entity
            /// </summary>
            [JsonProperty("polygon", NullValueHandling = NullValueHandling.Ignore)]
            public Polygon Polygon { get; set; }
        }

        /// <summary>
        /// JSON converter for HeightReference enum
        /// </summary>
        public class HeightReferenceConverter : JsonConverter
        {
            /// <summary>
            /// Can convert the type when it's a HeightReference
            /// </summary>
            /// <param name="objectType">object type to try to convert</param>
            /// <returns>true when it's a HeightReference</returns>
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(HeightReference);
            }

            /// <summary>
            /// Reads HeightReference from JSON; not implemented
            /// </summary>
            /// <param name="reader">reader; unused</param>
            /// <param name="objectType">object type; unused</param>
            /// <param name="existingValue">existing value; unused</param>
            /// <param name="serializer">serializer; unused</param>
            /// <returns>throws exception</returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Writes HeightReference value to JSON
            /// </summary>
            /// <param name="writer">JSON writer to use</param>
            /// <param name="value">HeightReference value to write</param>
            /// <param name="serializer">serializer; unused</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var heightReference = (HeightReference)value;
                switch (heightReference)
                {
                    case HeightReference.None:
                        writer.WriteValue("NONE");
                        break;
                    case HeightReference.RelativeToGround:
                        writer.WriteValue("RELATIVE_TO_GROUND");
                        break;
                    case HeightReference.ClampToGround:
                        writer.WriteValue("CLAMP_TO_GROUND");
                        break;
                    default:
                        Debug.Assert(false, "invalid height reference value");
                        break;
                }
            }
        }

        /// <summary>
        /// Height reference for positioning objects
        /// </summary>
        [JsonConverter(typeof(HeightReferenceConverter))]
        public enum HeightReference
        {
            /// <summary>
            /// No height reference; position absolute
            /// </summary>
            None,

            /// <summary>
            /// Position relative to ground
            /// </summary>
            RelativeToGround,

            /// <summary>
            /// Clamps to terrain
            /// </summary>
            ClampToGround,
        }

        /// <summary>
        /// CZML cylinder object
        /// </summary>
        public class Cylinder
        {
            /// <summary>
            /// Length (or more exactly, height) of cylinder
            /// </summary>
            [JsonProperty("length")]
            public double Length { get; set; }

            /// <summary>
            /// Radius at the top of the cylinder
            /// </summary>
            [JsonProperty("topRadius")]
            public double TopRadius { get; set; }

            /// <summary>
            /// Radius at the bottom
            /// </summary>
            [JsonProperty("bottomRadius")]
            public double BottomRadius { get; set; }

            /// <summary>
            /// Height reference of object
            /// </summary>
            [JsonProperty("heightReference")]
            public HeightReference HeightReference { get; set; } = HeightReference.None;

            /// <summary>
            /// Material used to show the cylinder
            /// </summary>
            [JsonProperty("material")]
            public Material Material { get; set; }

            /// <summary>
            /// Indicates if the cylinder should also be drawn with an outline
            /// </summary>
            [JsonProperty("outline")]
            public bool Outline { get; set; }

            /// <summary>
            /// Specifies the outline color
            /// </summary>
            [JsonProperty("outlineColor")]
            public Color OutlineColor { get; set; }
        }

        /// <summary>
        /// CZML polygon object
        /// </summary>
        public class Polygon
        {
            /// <summary>
            /// All positions of polygon points
            /// </summary>
            [JsonProperty("positions")]
            public Position Positions { get; set; }

            /// <summary>
            /// Height of polygon, when HeightReference is set to RelativeToGround
            /// </summary>
            [JsonProperty("height")]
            public double Height { get; set; }

            /// <summary>
            /// Extruded height in meters
            /// </summary>
            [JsonProperty("extrudedHeight")]
            public double ExtrudedHeight { get; set; }

            /// <summary>
            /// Height reference of object
            /// </summary>
            [JsonProperty("heightReference")]
            public HeightReference HeightReference { get; set; } = HeightReference.None;

            /// <summary>
            /// Material used to show the polygon
            /// </summary>
            [JsonProperty("material")]
            public Material Material { get; set; }

            /// <summary>
            /// Indicates if the polygon should also be drawn with an outline
            /// </summary>
            [JsonProperty("outline")]
            public bool Outline { get; set; }

            /// <summary>
            /// Specifies the outline color
            /// </summary>
            [JsonProperty("outlineColor")]
            public Color OutlineColor { get; set; }
        }

        /// <summary>
        /// A list of positions, containing multiple triplets of latitude, longitude and altitude
        /// values.
        /// </summary>
        public class Position
        {
            /// <summary>
            /// List of positions, 3 double values each, longitude/latitude/altitude
            /// </summary>
            [JsonProperty("cartographicDegrees")]
            public List<double> CartographicDegrees { get; set; } = new List<double>();

            /// <summary>
            /// Adds a new point to the list of cartographic positions
            /// </summary>
            /// <param name="latitude">latitude of point</param>
            /// <param name="longitude">longitude of point</param>
            /// <param name="height">height of point</param>
            internal void Add(double latitude, double longitude, double height)
            {
                // note the reversal of latitude and longitude in cartographicDegrees
                this.CartographicDegrees.Add(longitude);
                this.CartographicDegrees.Add(latitude);
                this.CartographicDegrees.Add(height);
            }
        }

        /// <summary>
        /// CZML material
        /// </summary>
        public class Material
        {
            /// <summary>
            /// Solid color for material
            /// </summary>
            [JsonProperty("solidColor")]
            public SolidColor SolidColor { get; set; }

            /// <summary>
            /// Creates a material from a solid color
            /// </summary>
            /// <param name="color">color to use</param>
            /// <returns>solid color material</returns>
            public static Material FromSolidColor(Color color)
            {
                return new Material
                {
                    SolidColor = new SolidColor
                    {
                        Color = color
                    }
                };
            }
        }

        /// <summary>
        /// Specifies a single solid color value
        /// </summary>
        public class SolidColor
        {
            /// <summary>
            /// Specifies the color value
            /// </summary>
            [JsonProperty("color")]
            public Color Color { get; set; }
        }

        /// <summary>
        /// Color value, specified by RGBA components in interval [0; 255]
        /// </summary>
        public class Color
        {
            /// <summary>
            /// Stores the color components
            /// </summary>
            [JsonProperty("rgba")]
            public int[] Rgba { get; set; } = new int[4];

            /// <summary>
            /// Creates a new
            /// </summary>
            /// <param name="r">red component</param>
            /// <param name="g">green component</param>
            /// <param name="b">blue component</param>
            /// <param name="a">alpha component</param>
            public Color(int r, int g, int b, int a = 0)
            {
                this.Rgba[0] = r;
                this.Rgba[1] = g;
                this.Rgba[2] = b;
                this.Rgba[3] = a;
            }
        }

        /// <summary>
        /// Writes a list of airspaces to a .czml text string
        /// </summary>
        /// <param name="name">name of czml document</param>
        /// <param name="allAirspaces">enumerable of airspaces</param>
        /// <param name="descriptionLines">list of description lines; may be empty</param>
        /// <returns>valid .czml file content</returns>
        public static string WriteCzml(string name, IEnumerable<Airspace.Airspace> allAirspaces, IEnumerable<string> descriptionLines)
        {
            string description = string.Join("\n", descriptionLines).Trim();
            description = description.Replace("\n", "<br/>");

            if (description.Length == 0)
            {
                description = null;
            }

            var objectList = new List<object>();
            objectList.Add(new CzmlPacketHeader(name, description));

            foreach (var airspace in allAirspaces)
            {
                var czmlObject = CzmlObjectFromAirspace(airspace);

                if (czmlObject == null)
                {
                    continue;
                }

                objectList.Add(czmlObject);
            }

            return JsonConvert.SerializeObject(objectList);
        }

        /// <summary>
        /// Creates a CzmlObject from given airspace
        /// </summary>
        /// <param name="airspace">airspace to use</param>
        /// <returns>created CZML object</returns>
        private static CzmlObject CzmlObjectFromAirspace(Airspace.Airspace airspace)
        {
            double height = HeightFromAltitude(airspace.Ceiling) - HeightFromAltitude(airspace.Floor);

            if (airspace.Geometry is Circle circle)
            {
                return new CzmlObject
                {
                    Name = airspace.Name,
                    Description = DescriptionFromAirspace(airspace),
                    Position = PositionFromCoord(circle.Center, airspace.Floor),
                    Cylinder = new Cylinder
                    {
                        BottomRadius = circle.Radius,
                        TopRadius = circle.Radius,
                        Length = height,
                        HeightReference = HeightReferenceFromAltitude(airspace.Floor),
                        Material = MaterialFromAirspace(airspace),
                        Outline = true,
                        OutlineColor = ColorFromAirspace(airspace, outlineColor: true),
                    }
                };
            }

            if (airspace.Geometry is Airspace.Polygon polygon)
            {
                Position position = GetPolygonPointsFromAirspacePolygon(polygon, airspace.Floor);

                if (!position.CartographicDegrees.Any())
                {
                    return null;
                }

                return new CzmlObject
                {
                    Name = airspace.Name,
                    Description = DescriptionFromAirspace(airspace),
                    Polygon = new Polygon
                    {
                        Positions = position,
                        Height = HeightFromAltitude(airspace.Floor),
                        ExtrudedHeight = height,
                        HeightReference = HeightReferenceFromAltitude(airspace.Floor),
                        Material = MaterialFromAirspace(airspace),
                        Outline = true,
                        OutlineColor = ColorFromAirspace(airspace, outlineColor: true),
                    }
                };
            }

            throw new FormatException("invalid airspace geometry");
        }

        /// <summary>
        /// Returns all polygon points from given airspace polygon object
        /// </summary>
        /// <param name="polygon">airspace polygon</param>
        /// <param name="floor">floor altitude</param>
        /// <returns>position object with all polygon points</returns>
        private static Position GetPolygonPointsFromAirspacePolygon(Airspace.Polygon polygon, Altitude floor)
        {
            double height = HeightFromAltitude(floor);

            var positions = new Position();

            foreach (var segment in polygon.Segments)
            {
                if (segment is Airspace.Polygon.PolygonPoint point)
                {
                    positions.Add(point.Point.Latitude, point.Point.Longitude, height);
                }

                if (segment is Airspace.Polygon.PolygonArc arc)
                {
                    var convertedArcSegment = ConvertPolygonArcToArcSegment(arc, height);
                    AddArcSegmentPolygonPoints(convertedArcSegment, positions, height);

                    positions.Add(arc.End.Latitude, arc.End.Longitude, height);
                }

                if (segment is Airspace.Polygon.PolygonArcSegment arcSegment)
                {
                    AddArcSegmentPolygonPoints(arcSegment, positions, height);
                }
            }

            return positions;
        }

        /// <summary>
        /// Converts a PolygonArc object to a PolygonArcSegment object and returns it. This is
        /// done by getting the arc segment radius from the distance center to start, the start
        /// and end angles by calculating the course from center to start end end point. When the
        /// distance from center to end point doesn't match the radius (the end point doesn't lie
        /// on the circle) the caller has to add the end point to the polygon, too.
        /// </summary>
        /// <param name="arc">arc object</param>
        /// <param name="height">height of arc</param>
        /// <returns>arg segment object</returns>
        private static Airspace.Polygon.PolygonArcSegment ConvertPolygonArcToArcSegment(
            Airspace.Polygon.PolygonArc arc,
            double height)
        {
            var start = arc.Start.ToMapPoint(height);
            var end = arc.End.ToMapPoint(height);
            var center = arc.Center.ToMapPoint(height);

            return new Airspace.Polygon.PolygonArcSegment
            {
                Center = arc.Center,
                Direction = arc.Direction,
                Radius = start.DistanceTo(arc.Center.ToMapPoint(height)),
                StartAngle = center.CourseTo(start),
                EndAngle = arc.Center.ToMapPoint(height).CourseTo(end)
            };
        }

        /// <summary>
        /// Generates all points for a PolygonArcSegment object and adds it to the positions
        /// </summary>
        /// <param name="arcSegment">arc segment to use</param>
        /// <param name="positions">positions to add to</param>
        /// <param name="height">height value</param>
        private static void AddArcSegmentPolygonPoints(
            Airspace.Polygon.PolygonArcSegment arcSegment,
            Position positions,
            double height)
        {
            var center = arcSegment.Center.ToMapPoint(height);

            bool isClockwise = arcSegment.Direction == Airspace.Polygon.ArcDirection.Clockwise;

            double endAngle = arcSegment.EndAngle;
            double startAngle = arcSegment.StartAngle;

            // adjust angles so that we don't have to deal with wrap around
            if (isClockwise && startAngle > endAngle)
            {
                endAngle += 360.0;
            }

            if (!isClockwise && startAngle < endAngle)
            {
                startAngle += 360.0;
            }

            bool isAtEnd = false;
            double step = (isClockwise ? 1.0 : -1.0) * ArcSegmentAngleStepByRadius(arcSegment.Radius);
            double bearing = startAngle;

            do
            {
                var point = center.PolarOffset(arcSegment.Radius, -bearing, height);
                positions.Add(point.Latitude, point.Longitude, height);

                bearing += step;
                isAtEnd = isClockwise
                    ? bearing > endAngle
                    : bearing < endAngle;
            }
            while (!isAtEnd);

            // add end point when the step angle didn't hit the end angle directly
            if (Math.Abs(bearing - arcSegment.EndAngle) > 1e-6)
            {
                var point = center.PolarOffset(arcSegment.Radius, -arcSegment.EndAngle, height);
                positions.Add(point.Latitude, point.Longitude, height);
            }
        }

        /// <summary>
        /// Depending on the size of the circle, use different number of angles for each arc
        /// segment line, to get smoother arc for larger circles.
        /// </summary>
        /// <param name="radius">arc radius, in meter</param>
        /// <returns>angle of a single line</returns>
        private static double ArcSegmentAngleStepByRadius(double radius)
        {
            if (radius < 10000.0)
            {
                return 5.0;
            }

            if (radius < 25000.0)
            {
                return 3.0;
            }

            if (radius < 50000.0)
            {
                return 2.0;
            }

            return 1.0;
        }

        /// <summary>
        /// Returns a formatted description for given airspace
        /// </summary>
        /// <param name="airspace">airspace to use</param>
        /// <returns>description text</returns>
        private static string DescriptionFromAirspace(Airspace.Airspace airspace)
        {
            var sb = new StringBuilder();

            sb.Append("Name: " + airspace.Name + "\n");
            sb.Append("Class: " + airspace.Class.ToString());
            if (!string.IsNullOrEmpty(airspace.AirspaceType))
            {
                sb.Append($" ({airspace.AirspaceType})");
            }

            sb.Append("\n");

            sb.Append($"{airspace.Floor} -> {airspace.Ceiling}\n");

            if (!string.IsNullOrEmpty(airspace.Description))
            {
                sb.Append($"Description: {airspace.Description}\n");
            }

            if (!string.IsNullOrEmpty(airspace.Frequency))
            {
                sb.Append($"Frequency: {airspace.Frequency}");
            }

            if (!string.IsNullOrEmpty(airspace.CallSign))
            {
                sb.Append($"Call sign: {airspace.CallSign}");
            }

            sb.Replace("\n\n", "\n");
            sb.Replace("\n", "<br/>");

            return sb.ToString();
        }

        /// <summary>
        /// Returns a position from given coordinates and altitude
        /// </summary>
        /// <param name="coord">coordinates object</param>
        /// <param name="altitude">altitude object</param>
        /// <returns>CZML position object</returns>
        private static Position PositionFromCoord(Coord coord, Altitude altitude)
        {
            return new Position
            {
                CartographicDegrees = new List<double>
                {
                    // note the reversal of latitude and longitude
                    coord.Longitude,
                    coord.Latitude,
                    HeightFromAltitude(altitude),
                }
            };
        }

        /// <summary>
        /// Returns height from altitude object
        /// </summary>
        /// <param name="altitude">altitude object</param>
        /// <returns>height value</returns>
        private static double HeightFromAltitude(Altitude altitude)
        {
            switch (altitude.Type)
            {
                case AltitudeType.GND:
                    return 0.0;
                case AltitudeType.Unlimited:
                    return UnlimitedAltitudeHeightInMeter;
                case AltitudeType.Textual:
                    return UnlimitedAltitudeHeightInMeter;
                case AltitudeType.AMSL:
                case AltitudeType.AGL:
                    return altitude.Value * Spatial.Constants.FactorFeetToMeter;
                case AltitudeType.FlightLevel:
                    return altitude.Value * 100.0 * Spatial.Constants.FactorFeetToMeter;
                default:
                    Debug.Assert(false, "invalid altitude type");
                    break;
            }

            return 0.0;
        }

        /// <summary>
        /// Returns a height reference for the given altitude
        /// </summary>
        /// <param name="altitude">altitude object</param>
        /// <returns>height reference value</returns>
        private static HeightReference HeightReferenceFromAltitude(Altitude altitude)
        {
            switch (altitude.Type)
            {
                case AltitudeType.GND:
                    return HeightReference.ClampToGround;
                case AltitudeType.Unlimited:
                case AltitudeType.Textual:
                case AltitudeType.AGL:
                    return HeightReference.RelativeToGround;
                case AltitudeType.AMSL:
                case AltitudeType.FlightLevel:
                default:
                    return HeightReference.None;
            }
        }

        /// <summary>
        /// Returns material object from given airspace
        /// </summary>
        /// <param name="airspace">airspace to use</param>
        /// <returns>material object</returns>
        private static Material MaterialFromAirspace(Airspace.Airspace airspace)
        {
            var color = ColorFromAirspace(airspace, outlineColor: false);
            return Material.FromSolidColor(color);
        }

        /// <summary>
        /// Calculates a color from given airspace; uses a default color when none was set.
        /// Outline colors are non-transparent and 20% darker, normal color is half transparent.
        /// </summary>
        /// <param name="airspace">airspace to use</param>
        /// <param name="outlineColor">true when outline color should be generated</param>
        /// <returns>color object</returns>
        private static Color ColorFromAirspace(Airspace.Airspace airspace, bool outlineColor)
        {
            string color = airspace.Color ?? DefaultAirspaceColor;

            if (!int.TryParse(color, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgba))
            {
                rgba = 0xc0c0c0;
            }

            int r = (rgba & 0xff0000) >> 16;
            int g = (rgba & 0xff00) >> 8;
            int b = rgba & 0xff;

            if (outlineColor)
            {
                r -= (int)(r * 0.2);
                g -= (int)(g * 0.2);
                b -= (int)(b * 0.2);
            }

            return new Color(r, g, b, outlineColor ? 255 : 128);
        }
    }
}
