namespace WhereToFly.Geo.Model
{
    /// <summary>
    /// Layer type
    /// </summary>
    public enum LayerType
    {
        /// <summary>
        /// The built-in location layer that shows all locations
        /// </summary>
        LocationLayer,

        /// <summary>
        /// The built-in track layer that shows all tracks
        /// </summary>
        TrackLayer,

        /// <summary>
        /// Layer containing Cesium OSM buildings
        /// </summary>
        OsmBuildingsLayer,

        /// <summary>
        /// A custom layer that shows a .czml file containing CesiumJS objects
        /// </summary>
        CzmlLayer,
    }
}
