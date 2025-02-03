using System.Text.Json.Serialization;
using WhereToFly.App.Services;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Serializers
{
    /// <summary>
    /// Serializer context for all model classes used in the app
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        UseStringEnumConverter = true,
        GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(Dictionary<LatLongKey, List<Location>>))]
    internal partial class ModelsJsonSerializerContext : JsonSerializerContext
    {
        // nothing to add here
    }
}
