using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable CA1819 // Properties should not return arrays

/// <summary>
/// C# object model for deserializing CZML JSON data.
/// </summary>
namespace WhereToFly.Geo.DataFormats.Czml
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
        public PositionList Position { get; set; }

        /// <summary>
        /// Point entity
        /// </summary>
        [JsonProperty("point", NullValueHandling = NullValueHandling.Ignore)]
        public Point Point { get; set; }

        /// <summary>
        /// Label entity
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public Label Label { get; set; }

        /// <summary>
        /// Billboard entity
        /// </summary>
        [JsonProperty("billboard", NullValueHandling = NullValueHandling.Ignore)]
        public Billboard Billboard { get; set; }

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
    /// JSON converter for HorizontalOrigin enum
    /// </summary>
    public class HorizontalOriginConverter : JsonConverter
    {
        /// <summary>
        /// Can convert the type when it's a HorizontalOrigin
        /// </summary>
        /// <param name="objectType">object type to try to convert</param>
        /// <returns>true when it's a HorizontalOrigin</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HorizontalOrigin);
        }

        /// <summary>
        /// Reads HorizontalOrigin from JSON; not implemented
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
        /// Writes HorizontalOrigin value to JSON
        /// </summary>
        /// <param name="writer">JSON writer to use</param>
        /// <param name="value">HorizontalOrigin value to write</param>
        /// <param name="serializer">serializer; unused</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var horizontalOrigin = (HorizontalOrigin)value;
            switch (horizontalOrigin)
            {
                case HorizontalOrigin.Center:
                    writer.WriteValue("CENTER");
                    break;
                case HorizontalOrigin.Left:
                    writer.WriteValue("LEFT");
                    break;
                case HorizontalOrigin.Right:
                    writer.WriteValue("RIGHT");
                    break;
                default:
                    Debug.Assert(false, "invalid horizontal origin value");
                    break;
            }
        }
    }

    /// <summary>
    /// The horizontal origin of a Billboard or Label object, relative to the position.
    /// </summary>
    [JsonConverter(typeof(HorizontalOriginConverter))]
    public enum HorizontalOrigin
    {
        /// <summary>
        /// Origin at the center of the anchor point
        /// </summary>
        Center,

        /// <summary>
        /// Origin at the left of the anchor point
        /// </summary>
        Left,

        /// <summary>
        /// Origin at the right of the anchor point
        /// </summary>
        Right,
    }

    /// <summary>
    /// JSON converter for VerticalOrigin enum
    /// </summary>
    public class VerticalOriginConverter : JsonConverter
    {
        /// <summary>
        /// Can convert the type when it's a VerticalOrigin
        /// </summary>
        /// <param name="objectType">object type to try to convert</param>
        /// <returns>true when it's a VerticalOrigin</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VerticalOrigin);
        }

        /// <summary>
        /// Reads VerticalOrigin from JSON; not implemented
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
        /// Writes VerticalOrigin value to JSON
        /// </summary>
        /// <param name="writer">JSON writer to use</param>
        /// <param name="value">VerticalOrigin value to write</param>
        /// <param name="serializer">serializer; unused</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var verticalOrigin = (VerticalOrigin)value;
            switch (verticalOrigin)
            {
                case VerticalOrigin.Center:
                    writer.WriteValue("CENTER");
                    break;
                case VerticalOrigin.Bottom:
                    writer.WriteValue("BOTTOM");
                    break;
                case VerticalOrigin.Baseline:
                    writer.WriteValue("BASELINE");
                    break;
                case VerticalOrigin.Top:
                    writer.WriteValue("TOP");
                    break;
                default:
                    Debug.Assert(false, "invalid vertical origin value");
                    break;
            }
        }
    }

    /// <summary>
    /// The vertical origin of a Billboard or Label object, relative to the position.
    /// </summary>
    [JsonConverter(typeof(VerticalOriginConverter))]
    public enum VerticalOrigin
    {
        /// <summary>
        /// Origin at the vertical center between Baseline and Top
        /// </summary>
        Center,

        /// <summary>
        /// Origin at the bottom of the anchor point
        /// </summary>
        Bottom,

        /// <summary>
        /// Origin at the baseline of the text
        /// </summary>
        Baseline,

        /// <summary>
        /// Origin at the vertical top
        /// </summary>
        Top,
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
    /// JSON converter for LabelStyle enum
    /// </summary>
    public class LabelStyleConverter : JsonConverter
    {
        /// <summary>
        /// Can convert the type when it's a LabelStyle
        /// </summary>
        /// <param name="objectType">object type to try to convert</param>
        /// <returns>true when it's a LabelStyle</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LabelStyle);
        }

        /// <summary>
        /// Reads LabelStyle from JSON; not implemented
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
        /// Writes LabelStyle value to JSON
        /// </summary>
        /// <param name="writer">JSON writer to use</param>
        /// <param name="value">LabelStyle value to write</param>
        /// <param name="serializer">serializer; unused</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var verticalOrigin = (LabelStyle)value;
            switch (verticalOrigin)
            {
                case LabelStyle.Fill:
                    writer.WriteValue("FILL");
                    break;
                case LabelStyle.Outline:
                    writer.WriteValue("OUTLINE");
                    break;
                case LabelStyle.FillAndOutline:
                    writer.WriteValue("FILL_AND_OUTLINE");
                    break;
                default:
                    Debug.Assert(false, "invalid label style value");
                    break;
            }
        }
    }

    /// <summary>
    /// Distance display condition; specifies if an entity is visible based on the viewer's
    /// distance.
    /// </summary>
    public class DistanceDisplayCondition
    {
        /// <summary>
        /// Distance display condition values
        /// </summary>
        [JsonProperty("distanceDisplayCondition")]
        public double[] DistanceDisplayConditionValues { get; set; } = new double[2];

        /// <summary>
        /// Creates a new distance display condition object
        /// </summary>
        /// <param name="nearDistance">near distance</param>
        /// <param name="farDistance">far distance</param>
        public DistanceDisplayCondition(double nearDistance, double farDistance)
        {
            this.DistanceDisplayConditionValues[0] = nearDistance;
            this.DistanceDisplayConditionValues[1] = farDistance;
        }
    }

    /// <summary>
    /// The label style of a Label object.
    /// </summary>
    [JsonConverter(typeof(LabelStyleConverter))]
    public enum LabelStyle
    {
        /// <summary>
        /// Filled label
        /// </summary>
        Fill,

        /// <summary>
        /// Only outline label
        /// </summary>
        Outline,

        /// <summary>
        /// Filled and outline label style
        /// </summary>
        FillAndOutline,
    }

    /// <summary>
    /// CZML label object
    /// </summary>
    public class Label
    {
        /// <summary>
        /// Label text
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Font name, e.g. "12pt Lucida Console"
        /// </summary>
        [JsonProperty("font", NullValueHandling = NullValueHandling.Ignore)]
        public string Font { get; set; }

        /// <summary>
        /// Label style
        /// </summary>
        [JsonProperty("style")]
        public LabelStyle LabelStyle { get; set; }

        /// <summary>
        /// Indicates if background is shown
        /// </summary>
        [JsonProperty("showBackground")]
        public bool ShowBackground { get; set; }

        /// <summary>
        /// Horizontal origin value for the label
        /// </summary>
        [JsonProperty("horizontalOrigin")]
        public HorizontalOrigin HorizontalOrigin { get; set; }

        /// <summary>
        /// Vertical origin value for the label
        /// </summary>
        [JsonProperty("verticalOrigin")]
        public VerticalOrigin VerticalOrigin { get; set; }

        /// <summary>
        /// Height reference of object
        /// </summary>
        [JsonProperty("heightReference")]
        public HeightReference HeightReference { get; set; } = HeightReference.None;

        /// <summary>
        /// Specifies the fill color
        /// </summary>
        [JsonProperty("fillColor", NullValueHandling = NullValueHandling.Ignore)]
        public Color FillColor { get; set; }

        /// <summary>
        /// Specifies the background color
        /// </summary>
        [JsonProperty("backgroundColor", NullValueHandling = NullValueHandling.Ignore)]
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Specifies the outline color
        /// </summary>
        [JsonProperty("outlineColor", NullValueHandling = NullValueHandling.Ignore)]
        public Color OutlineColor { get; set; }

        /// <summary>
        /// Outline width
        /// </summary>
        [JsonProperty("outlineWidth")]
        public double OutlineWidth { get; set; } = 1.0;

        /// <summary>
        /// Distance display condition
        /// </summary>
        [JsonProperty("distanceDisplayCondition", NullValueHandling = NullValueHandling.Ignore)]
        public DistanceDisplayCondition DistanceDisplayCondition { get; set; }
    }

    /// <summary>
    /// CZML billboard object
    /// </summary>
    public class Billboard
    {
        /// <summary>
        /// Billboard image URI
        /// </summary>
        [JsonProperty("image")]
        public string Image { get; set; }

        /// <summary>
        /// Horizontal origin value for the billboard
        /// </summary>
        [JsonProperty("horizontalOrigin")]
        public HorizontalOrigin HorizontalOrigin { get; set; } = HorizontalOrigin.Center;

        /// <summary>
        /// Vertical origin value for the billboard
        /// </summary>
        [JsonProperty("verticalOrigin")]
        public VerticalOrigin VerticalOrigin { get; set; } = VerticalOrigin.Center;

        /// <summary>
        /// Height reference of object
        /// </summary>
        [JsonProperty("heightReference")]
        public HeightReference HeightReference { get; set; } = HeightReference.None;

        /// <summary>
        /// Indicates if height and width are in meters (when true) or in pixels (when false)
        /// </summary>
        [JsonProperty("sizeInMeters")]
        public bool SizeInMeters { get; set; }

        /// <summary>
        /// Viewing distance where depth test is disabled for the billboard
        /// </summary>
        [JsonProperty("disableDepthTestDistance")]
        public double DisableDepthTestDistance { get; set; } = 0.0;
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
        /// Polyline width
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

        /// <summary>
        /// Distance display condition
        /// </summary>
        [JsonProperty("distanceDisplayCondition", NullValueHandling = NullValueHandling.Ignore)]
        public DistanceDisplayCondition DistanceDisplayCondition { get; set; }
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
    /// CZML polygon object.
    /// Note that clamping to ground is achieved when not setting Height and HeightReference property
    /// </summary>
    public class Polygon
    {
        /// <summary>
        /// All positions of polygon points
        /// </summary>
        [JsonProperty("positions")]
        public PositionList Positions { get; set; }

        /// <summary>
        /// Height of polygon, when HeightReference is set to RelativeToGround
        /// </summary>
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public double? Height { get; set; }

        /// <summary>
        /// Extruded height in meters
        /// </summary>
        [JsonProperty("extrudedHeight", NullValueHandling = NullValueHandling.Ignore)]
        public double? ExtrudedHeight { get; set; }

        /// <summary>
        /// Height reference of object; when null, is not set at all
        /// </summary>
        [JsonProperty("heightReference", NullValueHandling = NullValueHandling.Ignore)]
        public HeightReference? HeightReference { get; set; } = null;

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
    public class PositionList
    {
        /// <summary>
        /// Creates a new empty position list object
        /// </summary>
        public PositionList()
        {
        }

        /// <summary>
        /// Creates a new position list object with a single point
        /// </summary>
        /// <param name="latitude">latitude of point</param>
        /// <param name="longitude">longitude of point</param>
        /// <param name="height">height of point; optional</param>
        public PositionList(double latitude, double longitude, double? height)
        {
            this.Add(latitude, longitude, height);
        }

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
        /// <param name="height">height of point; optional</param>
        public void Add(double latitude, double longitude, double? height)
        {
            // note the reversal of latitude and longitude in cartographicDegrees
            this.CartographicDegrees.Add(longitude);
            this.CartographicDegrees.Add(latitude);
            this.CartographicDegrees.Add(height ?? 0.0);
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
        public Color(int r, int g, int b, int a = 255)
        {
            this.Rgba[0] = r;
            this.Rgba[1] = g;
            this.Rgba[2] = b;
            this.Rgba[3] = a;
        }
    }
}
