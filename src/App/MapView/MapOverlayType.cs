namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Type of map overlay to display over the map imagery
    /// </summary>
    public enum MapOverlayType
    {
        /// <summary>
        /// Displays no map overlay
        /// </summary>
        None = 0,

        /// <summary>
        /// Displays contour lines (100m apart)
        /// </summary>
        ContourLines = 1,

        /// <summary>
        /// Displays slope steepness and contour lines (100m apart)
        /// </summary>
        SlopeAndContourLines = 2,

        /// <summary>
        /// Displays thermal skyways from https://thermal.kk7.ch/
        /// </summary>
        ThermalSkywaysKk7 = 3,

        /// <summary>
        /// Displays Black Marble 2017 imagery courtesy NASA Earth Observatory
        /// </summary>
        BlackMarble = 4,
    }
}
