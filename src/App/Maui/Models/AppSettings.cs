using WhereToFly.App.MapView;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Models
{
    /// <summary>
    /// Settings for the app
    /// </summary>
    public record AppSettings
    {
        /// <summary>
        /// Last position shown in the app
        /// </summary>
        public MapPoint LastShownPosition { get; set; }
            = new MapPoint(0.0, 0.0);

        /// <summary>
        /// Last viewing distance of map, in meters from terrain
        /// </summary>
        public int LastViewingDistance { get; set; } = 5000;

        /// <summary>
        /// Map shading mode; determines at which hour in the day the sun shading is simulated
        /// </summary>
        public MapShadingMode ShadingMode { get; set; }
            = MapShadingMode.CurrentTime;

        /// <summary>
        /// Map imagery type; specifies the layer used to display over the terrain data
        /// </summary>
        public MapImageryType MapImageryType { get; set; }
            = MapImageryType.OpenStreetMap;

        /// <summary>
        /// Map overlay type; specifies the overlay display over the imagery layer; may be half
        /// transparent
        /// </summary>
        public MapOverlayType MapOverlayType { get; set; }
            = MapOverlayType.None;

        /// <summary>
        /// Specifies the coordinate display format throughout the app
        /// </summary>
        public CoordinateDisplayFormat CoordinateDisplayFormat { get; set; }
            = CoordinateDisplayFormat.Format_dd_mm_mmm;

        /// <summary>
        /// Specifies if entity clustering is used on the map
        /// </summary>
        public bool UseMapEntityClustering { get; set; } = true;

        /// <summary>
        /// Last location filter settings
        /// </summary>
        public LocationFilterSettings LastLocationFilterSettings { get; set; }
         = new LocationFilterSettings();

        /// <summary>
        /// Last used flying range parameters
        /// </summary>
        public FlyingRangeParameters LastFlyingRangeParameters { get; set; }
            = new FlyingRangeParameters();

        /// <summary>
        /// Last shown settings page
        /// </summary>
        public int LastShownSettingsPage { get; set; } = 0;

        /// <summary>
        /// Indicates if the flight planning disclaimer was already shown to the user
        /// </summary>
        public bool ShownFlightPlanningDisclaimer { get; set; } = false;

        /// <summary>
        /// Theme value for the app style
        /// </summary>
        public AppTheme AppTheme { get; set; } = AppTheme.Unspecified;

        /// <summary>
        /// The currently set compass target; may be null
        /// </summary>
        public CompassTarget? CurrentCompassTarget { get; set; }
    }
}
