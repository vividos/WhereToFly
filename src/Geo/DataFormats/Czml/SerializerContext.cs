using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhereToFly.Geo.DataFormats.Czml
{
    /// <summary>
    /// Serializer context for CZML object model
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(IEnumerable<CzmlBase>))]
    public partial class SerializerContext : JsonSerializerContext
    {
        // nothing to add here
    }
}
