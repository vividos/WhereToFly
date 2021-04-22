namespace WhereToFly.Geo.Model
{
    /// <summary>
    /// A layer that contains map objects, e.g. the location pins, tracks or any .czml that the
    /// user loads. Don't confuse with CesiumJS imagery layer.
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// Unique ID of layer
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the layer
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Layer description text
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Layer type
        /// </summary>
        public LayerType LayerType { get; set; } = LayerType.CzmlLayer;

        /// <summary>
        /// Indicates if the layer is currently visible
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Data for the layer, e.g. the actual .czml text. Only filled for LayerType.CzmlLayer.
        /// </summary>
        public string Data { get; set; } = null;
    }
}
