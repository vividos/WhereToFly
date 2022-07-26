using System;
using WhereToFly.App.MapView;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.Models
{
    /// <summary>
    /// Settings for the app
    /// </summary>
    public sealed class AppSettings : IEquatable<AppSettings>
    {
        /// <summary>
        /// Last position shown in the app
        /// </summary>
        public MapPoint LastShownPosition { get; set; }

        /// <summary>
        /// Last viewing distance of map, in meters from terrain
        /// </summary>
        public int LastViewingDistance { get; set; } = 5000;

        /// <summary>
        /// Map shading mode; determines at which hour in the day the sun shading is simulated
        /// </summary>
        public MapShadingMode ShadingMode { get; set; }

        /// <summary>
        /// Map imagery type; specifies the layer used to display over the terrain data
        /// </summary>
        public MapImageryType MapImageryType { get; set; }

        /// <summary>
        /// Map overlay type; specifies the overlay display over the imagery layer; may be half
        /// transparent
        /// </summary>
        public MapOverlayType MapOverlayType { get; set; }

        /// <summary>
        /// Specifies the coordinate display format throughout the app
        /// </summary>
        public CoordinateDisplayFormat CoordinateDisplayFormat { get; set; }

        /// <summary>
        /// Specifies if entity clustering is used on the map
        /// </summary>
        public bool UseMapEntityClustering { get; set; } = true;

        /// <summary>
        /// Last location filter settings
        /// </summary>
        public LocationFilterSettings LastLocationFilterSettings { get; set; }

        /// <summary>
        /// Last used flying range parameters
        /// </summary>
        public FlyingRangeParameters LastFlyingRangeParameters { get; set; }

        /// <summary>
        /// Last shown settings page
        /// </summary>
        public int LastShownSettingsPage { get; set; }

        /// <summary>
        /// Indicates if the flight planning disclaimer was already shown to the user
        /// </summary>
        public bool ShownFlightPlanningDisclaimer { get; set; }

        /// <summary>
        /// Theme value for the app style
        /// </summary>
        public Theme AppTheme { get; set; }

        /// <summary>
        /// The currently set compass target; may be null
        /// </summary>
        public CompassTarget CurrentCompassTarget { get; set; }

        /// <summary>
        /// Creates a new default app settings object
        /// </summary>
        public AppSettings()
        {
            this.LastShownPosition = new MapPoint(0.0, 0.0);
            this.ShadingMode = MapShadingMode.CurrentTime;
            this.MapImageryType = MapImageryType.OpenStreetMap;
            this.MapOverlayType = MapOverlayType.None;
            this.CoordinateDisplayFormat = CoordinateDisplayFormat.Format_dd_mm_mmm;
            this.LastLocationFilterSettings = new LocationFilterSettings();
            this.LastFlyingRangeParameters = new FlyingRangeParameters();
            this.LastShownSettingsPage = 0;
            this.ShownFlightPlanningDisclaimer = false;
            this.AppTheme = Theme.Device;
        }

        #region object overridables implementation

        /// <summary>
        /// Returns hash code for app resource URI
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode() =>
            (this.LastShownPosition,
            this.LastViewingDistance,
            this.ShadingMode,
            this.MapImageryType,
            this.MapOverlayType,
            this.CoordinateDisplayFormat,
            this.UseMapEntityClustering,
            this.LastLocationFilterSettings,
            this.LastFlyingRangeParameters,
            this.LastShownSettingsPage,
            this.ShownFlightPlanningDisclaimer,
            this.AppTheme,
            this.CurrentCompassTarget).GetHashCode();

        /// <summary>
        /// Compares this app settings to another object
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>true when equal app settings, false when not</returns>
        public override bool Equals(object obj) =>
            (obj is AppSettings appSettings) && this.Equals(appSettings);
        #endregion

        #region IEquatable implementation

        /// <summary>
        /// Compares this app settings to another app settings object
        /// </summary>
        /// <param name="other">app settings object to compare to</param>
        /// <returns>true when equal app settings, false when not</returns>
        public bool Equals(AppSettings other) =>
            other != null &&
            this.LastShownPosition.Equals(other.LastShownPosition) &&
            this.LastLocationFilterSettings.Equals(other.LastLocationFilterSettings) &&
            this.LastFlyingRangeParameters.Equals(other.LastFlyingRangeParameters) &&
            (this.LastViewingDistance,
            this.ShadingMode,
            this.MapImageryType,
            this.MapOverlayType,
            this.CoordinateDisplayFormat,
            this.UseMapEntityClustering,
            this.LastShownSettingsPage,
            this.ShownFlightPlanningDisclaimer,
            this.AppTheme,
            this.CurrentCompassTarget) ==
            (other.LastViewingDistance,
            other.ShadingMode,
            other.MapImageryType,
            other.MapOverlayType,
            other.CoordinateDisplayFormat,
            other.UseMapEntityClustering,
            other.LastShownSettingsPage,
            other.ShownFlightPlanningDisclaimer,
            other.AppTheme,
            other.CurrentCompassTarget);
        #endregion

        /// <summary>
        /// Equality operator
        /// </summary>
        /// <param name="left">left operator argument</param>
        /// <param name="right">right operator argument</param>
        /// <returns>true when objects are equal, false when not</returns>
        public static bool operator ==(AppSettings left, AppSettings right) =>
            Equals(left, right);

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="left">left operator argument</param>
        /// <param name="right">right operator argument</param>
        /// <returns>true when objects are inequal, false when not</returns>
        public static bool operator !=(AppSettings left, AppSettings right) =>
            !Equals(left, right);
    }
}
