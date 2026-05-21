using System;
using System.Text.Json.Serialization;

namespace WhereToFly.Shared.Model;

/// <summary>
/// Data for a Live Waypoint
/// </summary>
public class LiveWaypointData
{
    /// <summary>
    /// Live Waypoint ID
    /// </summary>
    [JsonPropertyName("id")]
    public string ID { get; set; }

    /// <summary>
    /// Time stamp of last update to live waypoint data
    /// </summary>
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; set; }

    /// <summary>
    /// Latitude value, positive values to the north
    /// </summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude value, positive values to the east
    /// </summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// Altitude value, if available; 0 means "clamp to ground"
    /// </summary>
    [JsonPropertyName("altitude")]
    public double Altitude { get; set; }

    /// <summary>
    /// Name of live waypoint
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description, in MarkDown format
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Details link, e.g. to external website
    /// </summary>
    [JsonPropertyName("detailsLink")]
    public string DetailsLink { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new live waypoint data object
    /// </summary>
    /// <param name="id">live waypoint ID</param>
    public LiveWaypointData(string id)
    {
        this.ID = id;
    }
}
