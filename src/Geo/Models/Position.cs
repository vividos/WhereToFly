using System;

namespace WhereToFly.Geo.Models;

/// <summary>
/// Position data
/// </summary>
public record Position
{
    /// <summary>
    /// Location of pilot in WGS84 coordinates
    /// </summary>
    public MapPoint? Location { get; set; }

    /// <summary>
    /// Timestamp of location
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }
}
