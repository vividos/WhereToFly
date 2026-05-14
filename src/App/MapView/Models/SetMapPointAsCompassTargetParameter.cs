namespace WhereToFly.App.MapView.Models;

/// <summary>
/// Parameter for SetMapPointAsCompassTarget JavaScript event
/// </summary>
internal record SetMapPointAsCompassTargetParameter
{
    /// <summary>
    /// Name of map point to set
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Latitude of map point to set
    /// </summary>
    public double Latitude { get; set; } = 0.0;

    /// <summary>
    /// Longitude of map point to set
    /// </summary>
    public double Longitude { get; set; } = 0.0;

    /// <summary>
    /// Altitude of map point to set
    /// </summary>
    public double? Altitude { get; set; }
}
