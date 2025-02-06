using System.Text.Json.Serialization;

namespace WhereToFly.WebApi.Logic.Serializers
{
    /// <summary>
    /// Serializer context for all model classes used in
    /// <see cref="Services.FindMeSpotTrackerDataService"/>.
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        UseStringEnumConverter = true,
        PropertyNameCaseInsensitive = true,
        GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(Services.FindMeSpot.Model.RootObject))]
    internal partial class FindMeSpotJsonSerializerContext :
        JsonSerializerContext
    {
        // nothing to add here
    }
}
