using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable CA1819 // Properties should not return arrays

// C# object model for deserializing CZML JSON data.
namespace WhereToFly.Geo.DataFormats.Czml
{
    /// <summary>
    /// CZML base object
    /// </summary>
    [JsonDerivedType(typeof(PacketHeader))]
    [JsonDerivedType(typeof(Object))]
    public class CzmlBase
    {
        // empty common base class
    }

    /// <summary>
    /// CZML packet header
    /// </summary>
    public class PacketHeader : CzmlBase
    {
        /// <summary>
        /// Document ID
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = "document";

        /// <summary>
        /// Packet name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Packet description; may be null
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; }

        /// <summary>
        /// Packet version
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Creates a new packet header
        /// </summary>
        /// <param name="name">name of document</param>
        /// <param name="description">description for packet</param>
        public PacketHeader(string name, string? description)
        {
            this.Name = name;
            this.Description = description;
        }
    }

    /// <summary>
    /// CZML object, containing various entity objects
    /// </summary>
    public class Object : CzmlBase
    {
        /// <summary>
        /// ID of object
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString("B");

        /// <summary>
        /// Object name
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Object description
        /// </summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>
        /// Position; used by Point, Cylinder, etc.
        /// </summary>
        [JsonPropertyName("position")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PositionList? Position { get; set; }

        /// <summary>
        /// Point entity
        /// </summary>
        [JsonPropertyName("point")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Point? Point { get; set; }

        /// <summary>
        /// Label entity
        /// </summary>
        [JsonPropertyName("label")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Label? Label { get; set; }

        /// <summary>
        /// Billboard entity
        /// </summary>
        [JsonPropertyName("billboard")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Billboard? Billboard { get; set; }

        /// <summary>
        /// Polyline entity
        /// </summary>
        [JsonPropertyName("polyline")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Polyline? Polyline { get; set; }

        /// <summary>
        /// Cylinder entity
        /// </summary>
        [JsonPropertyName("cylinder")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Cylinder? Cylinder { get; set; }

        /// <summary>
        /// Polygon entity
        /// </summary>
        [JsonPropertyName("polygon")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Polygon? Polygon { get; set; }

        /// <summary>
        /// Model entity
        /// </summary>
        [JsonPropertyName("model")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Model? Model { get; set; }
    }

    /// <summary>
    /// JSON converter for HeightReference enum
    /// </summary>
    public class HeightReferenceConverter : JsonConverter<HeightReference>
    {
        /// <summary>
        /// Reads HeightReference from JSON; not implemented
        /// </summary>
        /// <param name="reader">reader; unused</param>
        /// <param name="typeToConvert">object type; unused</param>
        /// <param name="options">serializer options; unused</param>
        /// <returns>throws exception</returns>
        public override HeightReference Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes HeightReference value to JSON
        /// </summary>
        /// <param name="writer">JSON writer to use</param>
        /// <param name="value">HeightReference value to write</param>
        /// <param name="options">serializer options; unused</param>
        public override void Write(
            Utf8JsonWriter writer,
            HeightReference value,
            JsonSerializerOptions options)
        {
            switch (value)
            {
                case HeightReference.None:
                    writer.WriteStringValue("NONE");
                    break;
                case HeightReference.RelativeToGround:
                    writer.WriteStringValue("RELATIVE_TO_GROUND");
                    break;
                case HeightReference.ClampToGround:
                    writer.WriteStringValue("CLAMP_TO_GROUND");
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
    public class HorizontalOriginConverter : JsonConverter<HorizontalOrigin>
    {
        /// <summary>
        /// Reads HorizontalOrigin from JSON; not implemented
        /// </summary>
        /// <param name="reader">reader; unused</param>
        /// <param name="typeToConvert">object type; unused</param>
        /// <param name="options">serializer options; unused</param>
        /// <returns>throws exception</returns>
        public override HorizontalOrigin Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes HorizontalOrigin value to JSON
        /// </summary>
        /// <param name="writer">JSON writer to use</param>
        /// <param name="value">HorizontalOrigin value to write</param>
        /// <param name="options">serializer options; unused</param>
        public override void Write(
            Utf8JsonWriter writer,
            HorizontalOrigin value,
            JsonSerializerOptions options)
        {
            switch (value)
            {
                case HorizontalOrigin.Center:
                    writer.WriteStringValue("CENTER");
                    break;
                case HorizontalOrigin.Left:
                    writer.WriteStringValue("LEFT");
                    break;
                case HorizontalOrigin.Right:
                    writer.WriteStringValue("RIGHT");
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
    public class VerticalOriginConverter : JsonConverter<VerticalOrigin>
    {
        /// <summary>
        /// Reads VerticalOrigin from JSON; not implemented
        /// </summary>
        /// <param name="reader">reader; unused</param>
        /// <param name="typeToConvert">object type; unused</param>
        /// <param name="options">serializer options; unused</param>
        /// <returns>throws exception</returns>
        public override VerticalOrigin Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes VerticalOrigin value to JSON
        /// </summary>
        /// <param name="writer">JSON writer to use</param>
        /// <param name="value">VerticalOrigin value to write</param>
        /// <param name="options">serializer options; unused</param>
        public override void Write(
            Utf8JsonWriter writer,
            VerticalOrigin value,
            JsonSerializerOptions options)
        {
            switch (value)
            {
                case VerticalOrigin.Center:
                    writer.WriteStringValue("CENTER");
                    break;
                case VerticalOrigin.Bottom:
                    writer.WriteStringValue("BOTTOM");
                    break;
                case VerticalOrigin.Baseline:
                    writer.WriteStringValue("BASELINE");
                    break;
                case VerticalOrigin.Top:
                    writer.WriteStringValue("TOP");
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
        [JsonPropertyName("pixelSize")]
        public double PixelSize { get; set; }

        /// <summary>
        /// Height reference of object
        /// </summary>
        [JsonPropertyName("heightReference")]
        public HeightReference HeightReference { get; set; } = HeightReference.None;

        /// <summary>
        /// Specifies the color
        /// </summary>
        [JsonPropertyName("color")]
        public Color? Color { get; set; }

        /// <summary>
        /// Specifies the outline color
        /// </summary>
        [JsonPropertyName("outlineColor")]
        public Color? OutlineColor { get; set; }

        /// <summary>
        /// Outline width
        /// </summary>
        [JsonPropertyName("outlineWidth")]
        public double? OutlineWidth { get; set; }
    }

    /// <summary>
    /// JSON converter for LabelStyle enum
    /// </summary>
    public class LabelStyleConverter : JsonConverter<LabelStyle>
    {
        /// <summary>
        /// Reads LabelStyle from JSON; not implemented
        /// </summary>
        /// <param name="reader">reader; unused</param>
        /// <param name="typeToConvert">object type; unused</param>
        /// <param name="options">serializer options; unused</param>
        /// <returns>throws exception</returns>
        public override LabelStyle Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes LabelStyle value to JSON
        /// </summary>
        /// <param name="writer">JSON writer to use</param>
        /// <param name="value">LabelStyle value to write</param>
        /// <param name="options">serializer options; unused</param>
        public override void Write(
            Utf8JsonWriter writer,
            LabelStyle value,
            JsonSerializerOptions options)
        {
            switch (value)
            {
                case LabelStyle.Fill:
                    writer.WriteStringValue("FILL");
                    break;
                case LabelStyle.Outline:
                    writer.WriteStringValue("OUTLINE");
                    break;
                case LabelStyle.FillAndOutline:
                    writer.WriteStringValue("FILL_AND_OUTLINE");
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
        [JsonPropertyName("distanceDisplayCondition")]
        public double[] DistanceDisplayConditionValues { get; set; } = new double[2];

        /// <summary>
        /// Creates a new distance display condition object
        /// </summary>
        /// <param name="nearDistance">near distance</param>
        /// <param name="farDistance">far distance</param>
        public DistanceDisplayCondition(double nearDistance = 0.0, double farDistance = double.MaxValue)
        {
            Debug.Assert(nearDistance < farDistance, "near distance must be smaller than the far distance");

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
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// Font name, e.g. "12pt Lucida Console"
        /// </summary>
        [JsonPropertyName("font")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Font { get; set; }

        /// <summary>
        /// Label style
        /// </summary>
        [JsonPropertyName("style")]
        public LabelStyle LabelStyle { get; set; }

        /// <summary>
        /// Indicates if background is shown
        /// </summary>
        [JsonPropertyName("showBackground")]
        public bool ShowBackground { get; set; }

        /// <summary>
        /// Horizontal origin value for the label
        /// </summary>
        [JsonPropertyName("horizontalOrigin")]
        public HorizontalOrigin HorizontalOrigin { get; set; }

        /// <summary>
        /// Vertical origin value for the label
        /// </summary>
        [JsonPropertyName("verticalOrigin")]
        public VerticalOrigin VerticalOrigin { get; set; }

        /// <summary>
        /// Height reference of object
        /// </summary>
        [JsonPropertyName("heightReference")]
        public HeightReference HeightReference { get; set; } = HeightReference.None;

        /// <summary>
        /// Specifies the fill color
        /// </summary>
        [JsonPropertyName("fillColor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Color? FillColor { get; set; }

        /// <summary>
        /// Specifies the background color
        /// </summary>
        [JsonPropertyName("backgroundColor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Color? BackgroundColor { get; set; }

        /// <summary>
        /// Specifies the outline color
        /// </summary>
        [JsonPropertyName("outlineColor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Color? OutlineColor { get; set; }

        /// <summary>
        /// Outline width
        /// </summary>
        [JsonPropertyName("outlineWidth")]
        public double OutlineWidth { get; set; } = 1.0;

        /// <summary>
        /// Distance display condition
        /// </summary>
        [JsonPropertyName("distanceDisplayCondition")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DistanceDisplayCondition? DistanceDisplayCondition { get; set; }
    }

    /// <summary>
    /// CZML billboard object
    /// </summary>
    public class Billboard
    {
        /// <summary>
        /// Billboard image URI
        /// </summary>
        [JsonPropertyName("image")]
        public string? Image { get; set; }

        /// <summary>
        /// Width of image, in meters or pixels
        /// </summary>
        [JsonPropertyName("width")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Width { get; set; }

        /// <summary>
        /// Height of image, in meters or pixels
        /// </summary>
        [JsonPropertyName("height")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Height { get; set; }

        /// <summary>
        /// Horizontal origin value for the billboard
        /// </summary>
        [JsonPropertyName("horizontalOrigin")]
        public HorizontalOrigin HorizontalOrigin { get; set; } = HorizontalOrigin.Center;

        /// <summary>
        /// Vertical origin value for the billboard
        /// </summary>
        [JsonPropertyName("verticalOrigin")]
        public VerticalOrigin VerticalOrigin { get; set; } = VerticalOrigin.Center;

        /// <summary>
        /// Height reference of object
        /// </summary>
        [JsonPropertyName("heightReference")]
        public HeightReference HeightReference { get; set; } = HeightReference.None;

        /// <summary>
        /// Indicates if height and width are in meters (when true) or in pixels (when false)
        /// </summary>
        [JsonPropertyName("sizeInMeters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? SizeInMeters { get; set; }

        /// <summary>
        /// Viewing distance where depth test is disabled for the billboard
        /// </summary>
        [JsonPropertyName("disableDepthTestDistance")]
        public double DisableDepthTestDistance { get; set; } = 0.0;

        /// <summary>
        /// Distance display condition
        /// </summary>
        [JsonPropertyName("distanceDisplayCondition")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DistanceDisplayCondition? DistanceDisplayCondition { get; set; }
    }

    /// <summary>
    /// CZML polyline object
    /// </summary>
    public class Polyline
    {
        /// <summary>
        /// All positions of polyline points
        /// </summary>
        [JsonPropertyName("positions")]
        public PositionList? Positions { get; set; }

        /// <summary>
        /// Polyline width
        /// </summary>
        [JsonPropertyName("width")]
        public double Width { get; set; }

        /// <summary>
        /// Indicates if polyline is clamped to ground
        /// </summary>
        [JsonPropertyName("clampToGround")]
        public bool ClampToGround { get; set; }

        /// <summary>
        /// Material used to show the polyline
        /// </summary>
        [JsonPropertyName("material")]
        public Material? Material { get; set; }

        /// <summary>
        /// Distance display condition
        /// </summary>
        [JsonPropertyName("distanceDisplayCondition")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DistanceDisplayCondition? DistanceDisplayCondition { get; set; }
    }

    /// <summary>
    /// CZML cylinder object
    /// </summary>
    public class Cylinder
    {
        /// <summary>
        /// Length (or more exactly, height) of cylinder
        /// </summary>
        [JsonPropertyName("length")]
        public double Length { get; set; }

        /// <summary>
        /// Radius at the top of the cylinder
        /// </summary>
        [JsonPropertyName("topRadius")]
        public double TopRadius { get; set; }

        /// <summary>
        /// Radius at the bottom
        /// </summary>
        [JsonPropertyName("bottomRadius")]
        public double BottomRadius { get; set; }

        /// <summary>
        /// Height reference of object
        /// </summary>
        [JsonPropertyName("heightReference")]
        public HeightReference HeightReference { get; set; } = HeightReference.None;

        /// <summary>
        /// Material used to show the cylinder
        /// </summary>
        [JsonPropertyName("material")]
        public Material? Material { get; set; }

        /// <summary>
        /// Indicates if the cylinder should also be drawn with an outline
        /// </summary>
        [JsonPropertyName("outline")]
        public bool Outline { get; set; }

        /// <summary>
        /// Specifies the outline color
        /// </summary>
        [JsonPropertyName("outlineColor")]
        public Color? OutlineColor { get; set; }
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
        [JsonPropertyName("positions")]
        public PositionList? Positions { get; set; }

        /// <summary>
        /// Height of polygon, when HeightReference is set to RelativeToGround
        /// </summary>
        [JsonPropertyName("height")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Height { get; set; }

        /// <summary>
        /// Extruded height in meters
        /// </summary>
        [JsonPropertyName("extrudedHeight")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? ExtrudedHeight { get; set; }

        /// <summary>
        /// Height reference of object; when null, is not set at all
        /// </summary>
        [JsonPropertyName("heightReference")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HeightReference? HeightReference { get; set; } = null;

        /// <summary>
        /// Material used to show the polygon
        /// </summary>
        [JsonPropertyName("material")]
        public Material? Material { get; set; }

        /// <summary>
        /// Indicates if the polygon should also be drawn with an outline
        /// </summary>
        [JsonPropertyName("outline")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Outline { get; set; }

        /// <summary>
        /// Specifies the outline color
        /// </summary>
        [JsonPropertyName("outlineColor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Color? OutlineColor { get; set; }
    }

    /// <summary>
    /// CZML model object.
    /// </summary>
    public class Model
    {
        /// <summary>
        /// The model's URI, usually an URL to a .glb file
        /// </summary>
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        /// <summary>
        /// Scale of model
        /// </summary>
        [JsonPropertyName("scale")]
        public double Scale { get; set; } = 1.0;

        /// <summary>
        /// Height reference of object; when null, is not set at all
        /// </summary>
        [JsonPropertyName("heightReference")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HeightReference? HeightReference { get; set; } = null;
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
        [JsonPropertyName("cartographicDegrees")]
        public List<double> CartographicDegrees { get; set; } = [];

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
        [JsonPropertyName("solidColor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SolidColor? SolidColor { get; set; }

        /// <summary>
        /// Image based material
        /// </summary>
        [JsonPropertyName("image")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ImageMaterial? Image { get; set; }

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
                    Color = color,
                },
            };
        }

        /// <summary>
        /// Creates an image based material from an image filename or Url
        /// </summary>
        /// <param name="imageFilenameOrUrl">image filename or Url to use</param>
        /// <returns>image material</returns>
        public static Material FromImageFilename(string imageFilenameOrUrl)
        {
            return new Material
            {
                Image = new ImageMaterial
                {
                    Uri = imageFilenameOrUrl,
                },
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
        [JsonPropertyName("color")]
        public Color? Color { get; set; }
    }

    /// <summary>
    /// Color value, specified by RGBA components in interval [0; 255]
    /// </summary>
    public class Color
    {
        /// <summary>
        /// Stores the color components
        /// </summary>
        [JsonPropertyName("rgba")]
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

        /// <summary>
        /// Returns a color object from given CSS color string, either in the format #RRGGBB or
        /// #RRGGBBAA.
        /// </summary>
        /// <param name="cssColor">CSS color string</param>
        /// <returns>color object</returns>
        public static Color FromCssColorString(string cssColor)
        {
            string color = cssColor.TrimStart('#');
            uint rgba = uint.Parse(color, System.Globalization.NumberStyles.HexNumber);

            if (color.Length == 6)
            {
                return new Color(
                    (int)((rgba >> 16) & 0xFF),
                    (int)((rgba >> 8) & 0xFF),
                    (int)(rgba & 0xFF),
                    255);
            }
            else if (color.Length == 8)
            {
                return new Color(
                    (int)((rgba >> 24) & 0xFF),
                    (int)((rgba >> 16) & 0xFF),
                    (int)((rgba >> 8) & 0xFF),
                    (int)(rgba & 0xFF));
            }

            throw new FormatException("invalid css color string: " + cssColor);
        }
    }

    /// <summary>
    /// Image based material
    /// </summary>
    public class ImageMaterial
    {
        /// <summary>
        /// Image URI
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
    }
}
