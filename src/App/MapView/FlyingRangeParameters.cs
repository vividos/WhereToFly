namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Parameters for the flying range calculation
    /// </summary>
    public record FlyingRangeParameters
    {
        /// <summary>
        /// Specifies the glide ratio, in km gliding per 1000m sinking.
        /// </summary>
        public double GlideRatio { get; set; } = 6.0;

        /// <summary>
        /// Glider speed, in km/h; needed for calculating glide factor against wind
        /// </summary>
        public double GliderSpeed { get; set; } = 38.0;

        /// <summary>
        /// Wind speed, in km/h
        /// </summary>
        public double WindSpeed { get; set; } = 0.0;

        /// <summary>
        /// Wind direction, in degrees; 0° means North
        /// </summary>
        public double WindDirection { get; set; } = 0.0;

        /// <summary>
        /// Altitude offset, in meter above selected point
        /// </summary>
        public int AltitudeOffset { get; set; } = 0;
    }
}
