using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable CA1819 // Properties should not return arrays

// C# object model for deserializing GeoJSON to C#.
// See: https://en.wikipedia.org/wiki/GeoJSON and RFC 7946.
// GeoJSON can contain Point, LineString, Polygon elements, as well as multi-part variants
// of these objects (MultiPoint, MultiLineString and MultiPolygon).
namespace WhereToFly.Geo.DataFormats.GeoJson
{
    /// <summary>
    /// Element base class for all GeoJSON objects
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(Feature), "Feature")]
    [JsonDerivedType(typeof(FeatureCollection), "FeatureCollection")]
    [JsonDerivedType(typeof(GeometryCollection), "GeometryCollection")]
    public class Element
    {
        /// <summary>
        /// Type of GeoJSON object
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore]
        public string? Type { get; set; }

        /// <summary>
        /// Element title; may be null
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Properties attached to this element; may be empty
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = [];

        /// <summary>
        /// Deserializes a GeoJSON formatted JSON text to an element hierarchy and returns it.
        /// </summary>
        /// <param name="geoJsonText">GeoJSON text</param>
        /// <returns>root element</returns>
        public static Element? Deserialize(string geoJsonText)
        {
            return JsonSerializer.Deserialize<Element>(
                geoJsonText,
                GeoJsonSerializerContext.Default.Element);
        }
    }

    /// <summary>
    /// Feature collection element, containing zero or more features
    /// </summary>
    public class FeatureCollection : Element
    {
        /// <summary>
        /// List of features contained in this collection
        /// </summary>
        [JsonPropertyName("features")]
        public Feature[]? FeatureList { get; set; }
    }

    /// <summary>
    /// Single feature element, containing a geometry
    /// </summary>
    public class Feature : Element
    {
        /// <summary>
        /// Geometry for this feature
        /// </summary>
        [JsonPropertyName("geometry")]
        public Geometry? Geometry { get; set; }
    }

    /// <summary>
    /// Geometry element base class
    /// </summary>
    [DebuggerDisplay("Geometry {Type}")]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(PointGeometry), "Point")]
    [JsonDerivedType(typeof(LineStringGeometry), "LineString")]
    [JsonDerivedType(typeof(PolygonGeometry), "Polygon")]
    [JsonDerivedType(typeof(MultiPointGeometry), "MultiPoint")]
    [JsonDerivedType(typeof(MultiLineStringGeometry), "MultiLineString")]
    [JsonDerivedType(typeof(MultiPolygonGeometry), "MultiPolygon")]
    public class Geometry
    {
        /// <summary>
        /// Type of GeoJSON object
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore]
        public string? Type { get; set; }
    }

    /// <summary>
    /// Geometry collection element
    /// </summary>
    public class GeometryCollection : Element
    {
        /// <summary>
        /// List of geometries
        /// </summary>
        [JsonPropertyName("geometries")]
        public Geometry[]? GeometryList { get; set; }
    }

    /// <summary>
    /// Point geometry, containing one point
    /// </summary>
    public class PointGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the point geometry
        /// </summary>
        [JsonPropertyName("coordinates")]
        public double[]? Coordinates { get; set; }
    }

    /// <summary>
    /// Line string geometry, containing a list of points
    /// </summary>
    public class LineStringGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the line string geometry
        /// </summary>
        [JsonPropertyName("coordinates")]
        public double[][]? Coordinates { get; set; }
    }

    /// <summary>
    /// Polygon geometry, containing one or more closed polygons
    /// </summary>
    public class PolygonGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the polygon geometry
        /// </summary>
        [JsonPropertyName("coordinates")]
        public double[][][]? Coordinates { get; set; }
    }

    /// <summary>
    /// Multi-point geometry, containing one or more points
    /// </summary>
    public class MultiPointGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the multi-point geometry
        /// </summary>
        [JsonPropertyName("coordinates")]
        public double[][]? Coordinates { get; set; }
    }

    /// <summary>
    /// Multi line string geometry, containing a list of line strings
    /// </summary>
    public class MultiLineStringGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the multi line string geometry
        /// </summary>
        [JsonPropertyName("coordinates")]
        public double[][][]? Coordinates { get; set; }
    }

    /// <summary>
    /// Multi-polygon geometry
    /// </summary>
    public class MultiPolygonGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the multi-polygon geometry
        /// </summary>
        [JsonPropertyName("coordinates")]
        public double[][][][]? Coordinates { get; set; }
    }
}
