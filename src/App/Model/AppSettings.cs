namespace WhereToFly.App.Model
{
    /// <summary>
    /// Settings for the app
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Last known position of app user
        /// </summary>
        public MapPoint LastKnownPosition { get; set; }

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
        /// Creates a new default app settings object
        /// </summary>
        public AppSettings()
        {
            this.LastKnownPosition = new MapPoint(0.0, 0.0);
            this.ShadingMode = MapShadingMode.CurrentTime;
            this.MapImageryType = MapImageryType.OpenStreetMap;
            this.MapOverlayType = MapOverlayType.None;
            this.CoordinateDisplayFormat = CoordinateDisplayFormat.Format_dd_mm_mmm;
            this.LastLocationListFilterText = string.Empty;
        }
    }
}
