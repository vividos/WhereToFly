namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Additional parameter for the OnUpdateLastShownLocation JavaScript event
    /// </summary>
    internal class UpdateLastShownLocationParameter : LongTapParameter
    {
        /// <summary>
        /// Current viewing distance from the terrain, in meters
        /// </summary>
        public int ViewingDistance { get; set; }
    }
}
