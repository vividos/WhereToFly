using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo.Serializers
{
    /// <summary>
    /// Serializer context for all Geo model classes
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(MapPoint))]
    [JsonSerializable(typeof(MapRectangle))]
    [JsonSerializable(typeof(TrackPoint))]
    [JsonSerializable(typeof(Location))]
    [JsonSerializable(typeof(Track))]
    [JsonSerializable(typeof(Layer))]
    public partial class GeoModelJsonSerializerContext : JsonSerializerContext
    {
        // nothing to add here
    }
}
