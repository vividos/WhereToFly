using System;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Model
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
        /// Last used filter text for location list
        /// </summary>
        public string LastLocationListFilterText { get; set; }

        /// <summary>
        /// Last used flying range parameters
        /// </summary>
        public FlyingRangeParameters LastFlyingRangeParameters { get; set; }

        /// <summary>
        /// Last shown settings page
        /// </summary>
        public int LastShownSettingsPage { get; set; }

        /// <summary>
        /// Last location list filter takeoff directions value
        /// </summary>
        public TakeoffDirections LastLocationListFilterTakeoffDirections { get; set; }

        /// <summary>
        /// Filter settings that determines if non-takeoff locations should also be shown
        /// </summary>
        public bool LocationListFilterShowNonTakeoffLocations { get; set; }

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
            this.LastLocationListFilterText = string.Empty;
            this.LastFlyingRangeParameters = new FlyingRangeParameters();
            this.LastShownSettingsPage = 0;
            this.LastLocationListFilterTakeoffDirections = TakeoffDirections.All;
            this.LocationListFilterShowNonTakeoffLocations = true;
        }

        #region object overridables implementation
        /// <summary>
        /// Returns hash code for app resource URI
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode() =>
            (this.LastShownPosition,
            this.ShadingMode,
            this.MapImageryType,
            this.MapOverlayType,
            this.CoordinateDisplayFormat,
            this.LastLocationListFilterText,
            this.LastFlyingRangeParameters,
            this.LastShownSettingsPage,
            this.LastLocationListFilterTakeoffDirections,
            this.LocationListFilterShowNonTakeoffLocations).GetHashCode();

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
            this.LastFlyingRangeParameters.Equals(other.LastFlyingRangeParameters) &&
            (this.ShadingMode,
            this.MapImageryType,
            this.MapOverlayType,
            this.CoordinateDisplayFormat,
            this.LastLocationListFilterText,
            this.LastShownSettingsPage,
            this.LastLocationListFilterTakeoffDirections,
            this.LocationListFilterShowNonTakeoffLocations) ==
            (other.ShadingMode,
            other.MapImageryType,
            other.MapOverlayType,
            other.CoordinateDisplayFormat,
            other.LastLocationListFilterText,
            other.LastShownSettingsPage,
            other.LastLocationListFilterTakeoffDirections,
            other.LocationListFilterShowNonTakeoffLocations);
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
