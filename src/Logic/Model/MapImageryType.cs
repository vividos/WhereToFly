namespace WhereToFly.App.Logic.Model
{
    /// <summary>
    /// Type of map imagery used to lay over the terrain
    /// </summary>
    public enum MapImageryType
    {
        /// <summary>
        /// OpenStreetMap imagery tiles are displayed; the OSM layer often contains many details
        /// needed for hiking.
        /// </summary>
        OpenStreetMap = 0,

        /// <summary>
        /// Bing Maps aerials with lames imagery tiles are displayed.
        /// </summary>
        BingMapsAerialWithLabels = 1,
    }
}
