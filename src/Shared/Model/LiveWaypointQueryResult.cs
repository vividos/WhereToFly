using System;
using System.Text.Json.Serialization;

namespace WhereToFly.Shared.Model;

/// <summary>
/// Query result for live waypoint data
/// </summary>
[Serializable]
public sealed class LiveWaypointQueryResult
{
    /// <summary>
    /// Live waypoint data; may be null when it couldn't be retrieved yet
    /// </summary>
    [JsonPropertyName("data")]
    public LiveWaypointData? Data { get; set; }

    /// <summary>
    /// Date where next request for the requested live waypoint will be most probably
    /// available
    /// </summary>
    [JsonPropertyName("nextRequestDate")]
    public DateTimeOffset NextRequestDate { get; set; }
}
