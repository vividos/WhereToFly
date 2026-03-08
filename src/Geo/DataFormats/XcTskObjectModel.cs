using System.Text.Json.Serialization;

#pragma warning disable SA1402 // File may only contain a single type

namespace WhereToFly.Geo.DataFormats;

/// <summary>
/// Root document
/// </summary>
public class XcTskDocument
{
    /// <summary>
    /// Task type; currently only "CLASSIC"
    /// </summary>
    [JsonPropertyName("taskType")]
    public string TaskType { get; set; } = "CLASSIC";

    /// <summary>
    /// Version number, either 1 or 2
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    /// <summary>
    /// Earth model, optional; one of "WGS84", "FAI_SPHERE"
    /// </summary>
    [JsonPropertyName("earthModel")]
    public string EarthModel { get; set; } = string.Empty;

    /// <summary>
    /// List of turnpoints; required
    /// </summary>
    [JsonPropertyName("turnpoints")]
    public Turnpoint[] Turnpoints { get; set; } = [];

    /// <summary>
    /// Infos about takeoff; optional
    /// </summary>
    [JsonPropertyName("takeoff")]
    public Takeoff? Takeoff { get; set; }

    /// <summary>
    /// Infos about the start of speed section; optional
    /// </summary>
    [JsonPropertyName("sss")]
    public StartOfSpeedSection? StartOfSpeedSection { get; set; }

    /// <summary>
    /// Infos about the goal; optional
    /// </summary>
    [JsonPropertyName("goal")]
    public Goal? Goal { get; set; }
}

/// <summary>
/// Infos about a turnpoint
/// </summary>
public class Turnpoint
{
    /// <summary>
    /// Turnpoint type; one of "TAKEOFF", "SSS", "ESS" or empty for waypoints or goal
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Turnpoint radius in meter; required
    /// </summary>
    [JsonPropertyName("radius")]
    public int Radius { get; set; }

    /// <summary>
    /// Waypoint associated with turnpoint; required
    /// </summary>
    [JsonPropertyName("waypoint")]
    public Waypoint Waypoint { get; set; } = new();
}

/// <summary>
/// Infos about a waypoint
/// </summary>
public class Waypoint
{
    /// <summary>
    /// Waypoint name; required
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Waypoint description; optional
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Latitude in degrees
    /// </summary>
    [JsonPropertyName("lat")]
    public float Latitude { get; set; }

    /// <summary>
    /// Longitude in degrees
    /// </summary>
    [JsonPropertyName("lon")]
    public float Longitude { get; set; }

    /// <summary>
    /// Altitude in meter AMSL
    /// </summary>
    [JsonPropertyName("altSmoothed")]
    public int Altitude { get; set; }
}

/// <summary>
/// Infos about takeoff
/// </summary>
public class Takeoff
{
    /// <summary>
    /// Takeoff window open time; optional
    /// </summary>
    [JsonPropertyName("timeOpen")]
    public string? TimeOpen { get; set; }

    /// <summary>
    /// Takeoff window close time; optional
    /// </summary>
    [JsonPropertyName("timeClose")]
    public string? TimeClose { get; set; }
}

/// <summary>
/// Infos about the start of speed section
/// </summary>
public class StartOfSpeedSection
{
    /// <summary>
    /// SSS type, one of "RACE", "ELAPSED-TIME"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "RACE";

    /// <summary>
    /// SSS direction, "ENTER", "EXIT"; obsolete
    /// </summary>
    [JsonPropertyName("direction")]
    public string Direction { get; set; } = "ENTER";

    /// <summary>
    /// Array of time gates. When the type is "RACE", contains one start
    /// time.
    /// </summary>
    [JsonPropertyName("timeGates")]
    public string[] TimeGates { get; set; } = [];
}

/// <summary>
/// Infos about goal
/// </summary>
public class Goal
{
    /// <summary>
    /// Goal type; one of "CYLINDER", "LINE", optional
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "CYLINDER";

    /// <summary>
    /// Deadline time; optional
    /// </summary>
    [JsonPropertyName("deadline")]
    public string? Deadline { get; set; }
}
