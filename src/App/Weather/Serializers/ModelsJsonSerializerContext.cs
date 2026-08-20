using System.Text.Json.Serialization;
using WhereToFly.App.Weather.Models;

namespace WhereToFly.App.Weather.Serializers;

/// <summary>
/// Serializer context for all model classes used in the app
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = false,
    UseStringEnumConverter = true,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(List<WeatherIconDescription>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class ModelsJsonSerializerContext : JsonSerializerContext
{
    // nothing to add here
}
