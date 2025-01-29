using System.Text.Json.Serialization;

namespace WhereToFly.Geo.DataFormats.GeoJson
{
    /// <summary>
    /// Serializer context for GeoJSON format.
    /// AllowOutOfOrderMetadataProperties is needed since the type discriminator for GeoJSON could
    /// appear everywhere in the Element and Geomeotry JSON object. See also:
    /// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism
    /// </summary>
    [JsonSourceGenerationOptions(
        UseStringEnumConverter = true,
        AllowOutOfOrderMetadataProperties = true,
        GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(Element))]
    [JsonSerializable(typeof(Feature))]
    [JsonSerializable(typeof(FeatureCollection))]
    [JsonSerializable(typeof(GeometryCollection))]
    [JsonSerializable(typeof(Geometry))]
    [JsonSerializable(typeof(PointGeometry))]
    [JsonSerializable(typeof(LineStringGeometry))]
    [JsonSerializable(typeof(PolygonGeometry))]
    [JsonSerializable(typeof(MultiPointGeometry))]
    [JsonSerializable(typeof(MultiLineStringGeometry))]
    [JsonSerializable(typeof(MultiPolygonGeometry))]
    internal partial class GeoJsonSerializerContext :
        JsonSerializerContext
    {
        // nothing to add here
    }
}
