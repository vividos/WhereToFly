namespace WhereToFly.App.MapView.Models
{
    /// <summary>
    /// Parameter for AddFindResult JavaScript event
    /// </summary>
    internal record AddFindResultParameter
    {
        /// <summary>
        /// Name of find result to add
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Latitude of map point to add
        /// </summary>
        public double Latitude { get; set; } = 0.0;

        /// <summary>
        /// Longitude of map point to add
        /// </summary>
        public double Longitude { get; set; } = 0.0;

        /// <summary>
        /// Altitude of map point to add
        /// </summary>
        public double? Altitude { get; set; }
    }
}
