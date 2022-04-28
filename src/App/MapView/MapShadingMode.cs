namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Specifies how the map is shaded in terms of clock hour
    /// </summary>
    public enum MapShadingMode
    {
        /// <summary>
        /// Display map shading with time fixed at 10 AM
        /// </summary>
        Fixed10Am = 0,

        /// <summary>
        /// Display map shading with time fixed at 3 PM
        /// </summary>
        Fixed3Pm = 1,

        /// <summary>
        /// Display map shading based on current time
        /// </summary>
        CurrentTime = 2,

        /// <summary>
        /// Display map shading based on current time, but 6 hours ahead
        /// </summary>
        Ahead6Hours = 3,

        /// <summary>
        /// No map shading is used, all mountains are lit equally
        /// </summary>
        None = 4,
    }
}
