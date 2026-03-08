using System.Text.Json.Serialization;
using WhereToFly.Geo.DataFormats;

namespace WhereToFly.Geo.Serializers;

/// <summary>
/// Serializer context for .xctsk object model classes
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(XcTskDocument))]
public partial class XcTskObjectModelJsonSerializerContext :
    JsonSerializerContext
{
    // nothing to add here
}
