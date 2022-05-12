namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Parameter for OnLongTap JavaScript event
    /// </summary>
    internal class LongTapParameter
    {
        /// <summary>
        /// Latitude of map point where long tap occured
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude of map point where long tap occured
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Altitude of map point where long tap occured
        /// </summary>
        public double Altitude { get; set; }
    }
}
