namespace WhereToFly.Logic.Model
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
        /// Map overlay type; specifies the layer used to display over the terrain data
        /// </summary>
        public MapOverlayType MapOverlayType { get; set; }

        /// <summary>
        /// Specifies the coordinate display format throughout the app
        /// </summary>
        public CoordinateDisplayFormat CoordinateDisplayFormat { get; set; }

        /// <summary>
        /// Creates a new default app settings object
        /// </summary>
        public AppSettings()
        {
            this.LastKnownPosition = new MapPoint(0.0, 0.0);
            this.ShadingMode = MapShadingMode.CurrentTime;
            this.MapOverlayType = MapOverlayType.OpenStreetMap;
            this.CoordinateDisplayFormat = CoordinateDisplayFormat.Format_dd_mm_mmm;
        }
    }
}
