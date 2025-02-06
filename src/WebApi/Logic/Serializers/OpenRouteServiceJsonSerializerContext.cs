using System.Text.Json.Serialization;
using WhereToFly.WebApi.Logic.TourPlanning;

namespace WhereToFly.WebApi.Logic.Serializers
{
    /// <summary>
    /// Serializer context for all model classes used in <see cref="OpenRouteService"/>.
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        UseStringEnumConverter = true,
        GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(OpenRouteService.ResultObject))]
    [JsonSerializable(typeof(OpenRouteService.ErrorInfo))]
    internal partial class OpenRouteServiceJsonSerializerContext :
        JsonSerializerContext
    {
        // nothing to add here
    }
}
