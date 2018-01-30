namespace WhereToFly.Logic.Model
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
        /// Displays slope steepness and contour lines (100m apart)
        /// </summary>
        SlopeAndContourLines = 1,

        /// <summary>
        /// Displays thermal skyways from https://thermal.kk7.ch/
        /// </summary>
        ThermalSkywaysKk7 = 2,

        /// <summary>
        /// Displays Black Marble imagery courtesy NASA Earth Observatory
        /// </summary>
        BlackMarble = 3,
    }
}
