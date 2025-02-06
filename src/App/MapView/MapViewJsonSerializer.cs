using System.Text.Json.Serialization;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Serializer context for <see cref="MapView"/>
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(double[]))]
    [JsonSerializable(typeof(AddFindResultParameter))]
    [JsonSerializable(typeof(MapRectangle))]
    [JsonSerializable(typeof(LongTapParameter))]
    [JsonSerializable(typeof(UpdateLastShownLocationParameter))]
    [JsonSerializable(typeof(WebMessage))]
    internal partial class MapViewJsonSerializerContext : JsonSerializerContext
    {
        // nothing to add here
    }
}
