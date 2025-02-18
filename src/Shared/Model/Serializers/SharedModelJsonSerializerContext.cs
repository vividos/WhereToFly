using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WhereToFly.Geo.Model;

namespace WhereToFly.Shared.Model.Serializers
{
    /// <summary>
    /// Serializer context for all model classes used in the communication with the backend
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(AppConfig))]
    [JsonSerializable(typeof(AppResourceUri))]
    [JsonSerializable(typeof(DateTimeOffset))]
    [JsonSerializable(typeof(LiveTrackQueryResult))]
    [JsonSerializable(typeof(LiveWaypointQueryResult))]
    [JsonSerializable(typeof(PlanTourParameters))]
    [JsonSerializable(typeof(PlannedTour))]
    [JsonSerializable(typeof(IEnumerable<Location>))]
    public partial class SharedModelJsonSerializerContext : JsonSerializerContext
    {
        // nothing to add here
    }
}
