using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable CA1819 // Properties should not return arrays

/// <summary>
/// C# object model for deserializing GeoJSON to C#.
/// See: https://en.wikipedia.org/wiki/GeoJSON and RFC 7946.
/// GeoJSON can contain Point, LineString, Polygon elements, as well as multi-part variants
/// of these objects (MultiPoint, MultiLineString and MultiPolygon).
/// </summary>
namespace WhereToFly.Geo.DataFormats.GeoJson
{
    /// <summary>
    /// Element base class for all GeoJSON objects
    /// </summary>
    public class Element
    {
        /// <summary>
        /// Type of GeoJSON object
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Element title; may be null
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Properties attached to this element; may be empty
        /// </summary>
        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// Deserializes a GeoJSON formatted JSON text to an element hierarchy and returns it.
        /// </summary>
        /// <param name="geoJsonText">GeoJSON text</param>
        /// <returns>root element</returns>
        public static Element Deserialize(string geoJsonText)
        {
            return JsonConvert.DeserializeObject<Element>(
                geoJsonText,
                new JsonSerializerSettings
                {
                    Converters =
                    {
                        new ElementJsonDecoder(),
                    },
                });
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
        [JsonProperty("features")]
        public Feature[] FeatureList { get; set; }
    }

    /// <summary>
    /// Single feature element, containing a geometry
    /// </summary>
    public class Feature : Element
    {
        /// <summary>
        /// Geometry for this feature
        /// </summary>
        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }
    }

    /// <summary>
    /// Geometry element; technically the Geometry element may only appear in a feature, but
    /// to keep parsing simple, it is derived from Element, too.
    /// </summary>
    public class Geometry : Element
    {
    }

    /// <summary>
    /// Geometry collection element
    /// </summary>
    public class GeometryCollection : Element
    {
        /// <summary>
        /// List of geometries
        /// </summary>
        [JsonProperty("geometries")]
        public Geometry[] GeometryList { get; set; }
    }

    /// <summary>
    /// Point geometry, containing one point
    /// </summary>
    public class PointGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the point geometry
        /// </summary>
        [JsonProperty("coordinates")]
        public double[] Coordinates { get; set; }
    }

    /// <summary>
    /// Line string geometry, containing a list of points
    /// </summary>
    public class LineStringGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the line string geometry
        /// </summary>
        [JsonProperty("coordinates")]
        public double[][] Coordinates { get; set; }
    }

    /// <summary>
    /// Polygon geometry, containing one or more closed polygons
    /// </summary>
    public class PolygonGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the polygon geometry
        /// </summary>
        [JsonProperty("coordinates")]
        public double[][][] Coordinates { get; set; }
    }

    /// <summary>
    /// Multi-point geometry, containing one or more points
    /// </summary>
    public class MultiPointGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the multi-point geometry
        /// </summary>
        [JsonProperty("coordinates")]
        public double[][] Coordinates { get; set; }
    }

    /// <summary>
    /// Multi line string geometry, containing a list of line strings
    /// </summary>
    public class MultiLineStringGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the multi line string geometry
        /// </summary>
        [JsonProperty("coordinates")]
        public double[][][] Coordinates { get; set; }
    }

    /// <summary>
    /// Multi-polygon geometry
    /// </summary>
    public class MultiPolygonGeometry : Geometry
    {
        /// <summary>
        /// Coordinates for the multi-polygon geometry
        /// </summary>
        [JsonProperty("coordinates")]
        public double[][][][] Coordinates { get; set; }
    }

    /// <summary>
    /// Deserializer for Element derived objects; see also:
    /// https://www.codeproject.com/Tips/1206080/Complex-deserialization-of-objects-from-JSON
    /// </summary>
    public class ElementJsonDecoder : JsonConverter
    {
        /// <summary>
        /// Indicates if writing is supported (no, it isn't)
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Determines if a given type can be converted by this converter
        /// </summary>
        /// <param name="objectType">the object's type</param>
        /// <returns>true when convertable, false when not</returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(Element).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Writes JSON; not implemented
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="value">value to write</param>
        /// <param name="serializer">JSON serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => throw new NotSupportedException("writing not supported");

        /// <summary>
        /// Reads JSON
        /// </summary>
        /// <param name="reader">JSON reader</param>
        /// <param name="objectType">the object's type</param>
        /// <param name="existingValue">existing object; unused</param>
        /// <param name="serializer">JSON serializer</param>
        /// <returns>read object</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            // load JObject from stream
            JObject jsonObject = JObject.Load(reader);
            if (jsonObject == null)
            {
                return null;
            }

            string type = jsonObject.Value<string>("type");
            if (type == null)
            {
                throw new ArgumentException($"Unable to parse value object");
            }

            object target = CreateTargetObjectFromType(type);

            serializer.Populate(jsonObject.CreateReader(), target);
            return target;
        }

        /// <summary>
        /// Creates a target element object, based on the type string
        /// </summary>
        /// <param name="type">GeoJSON type string</param>
        /// <returns>target object</returns>
        private static object CreateTargetObjectFromType(string type)
        {
            return type switch
            {
                "Feature" => new Feature(),
                "FeatureCollection" => new FeatureCollection(),
                "GeometryCollection" => new GeometryCollection(),
                "Point" => new PointGeometry(),
                "LineString" => new LineStringGeometry(),
                "Polygon" => new PolygonGeometry(),
                "MultiPoint" => new MultiPointGeometry(),
                "MultiLineString" => new MultiLineStringGeometry(),
                "MultiPolygon" => new MultiPolygonGeometry(),
                _ => throw new ArgumentException($"Unknown geometry type '{type}'"),
            };
        }
    }
}
