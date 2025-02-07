using System.Text.Json.Serialization;
using WhereToFly.App.Models;
using WhereToFly.App.Services;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Serializers
{
    /// <summary>
    /// Serializer context for all model classes used in the app
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        UseStringEnumConverter = true,
        GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(AppConfig))]
    [JsonSerializable(typeof(AppSettings))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(List<WeatherIconDescription>))]
    [JsonSerializable(typeof(Dictionary<LatLongKey, List<Location>>))]
    internal partial class ModelsJsonSerializerContext : JsonSerializerContext
    {
        // nothing to add here
    }
}
