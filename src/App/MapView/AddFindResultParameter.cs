namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Parameter for AddFindResult JavaScript event
    /// </summary>
    internal class AddFindResultParameter
    {
        /// <summary>
        /// Name of find result to add
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Latitude of map point to add
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude of map point to add
        /// </summary>
        public double Longitude { get; set; }
    }
}
