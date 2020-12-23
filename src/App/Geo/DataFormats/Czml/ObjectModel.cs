using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable SA1402 // File may only contain a single type

/// <summary>
/// C# object model for deserializing CZML JSON data.
/// </summary>
namespace WhereToFly.App.Geo.DataFormats.Czml
{
    /// <summary>
    /// CZML packet header
    /// </summary>
    public class PacketHeader
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
        public PacketHeader(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
    }

    /// <summary>
    /// CZML object, containing various entity objects
    /// </summary>
    public class Object
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
        /// Point entity
        /// </summary>
        [JsonProperty("point", NullValueHandling = NullValueHandling.Ignore)]
        public Point Point { get; set; }

        /// <summary>
        /// Polyline entity
        /// </summary>
        [JsonProperty("polyline", NullValueHandling = NullValueHandling.Ignore)]
        public Polyline Polyline { get; set; }

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
    /// CZML point object
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Pixel size
        /// </summary>
        [JsonProperty("pixelSize")]
        public double PixelSize { get; set; }

        /// <summary>
        /// Height reference of object
        /// </summary>
        [JsonProperty("heightReference")]
        public HeightReference HeightReference { get; set; } = HeightReference.None;

        /// <summary>
        /// Specifies the color
        /// </summary>
        [JsonProperty("color")]
        public Color Color { get; set; }

        /// <summary>
        /// Specifies the outline color
        /// </summary>
        [JsonProperty("outlineColor")]
        public Color OutlineColor { get; set; }

        /// <summary>
        /// Outline width
        /// </summary>
        [JsonProperty("outlineWidth")]
        public double OutlineWidth { get; set; }
    }

    /// <summary>
    /// CZML polyline object
    /// </summary>
    public class Polyline
    {
        /// <summary>
        /// All positions of polyline points
        /// </summary>
        [JsonProperty("positions")]
        public PositionList Positions { get; set; }

        /// <summary>
        /// Width
        /// </summary>
        [JsonProperty("width")]
        public double Width { get; set; }

        /// <summary>
        /// Indicates if polyline is clamped to ground
        /// </summary>
        [JsonProperty("clampToGround")]
        public bool ClampToGround { get; set; }

        /// <summary>
        /// Material used to show the polyline
        /// </summary>
        [JsonProperty("material")]
        public Material Material { get; set; }
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
        /// Creates a new color object
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
}
