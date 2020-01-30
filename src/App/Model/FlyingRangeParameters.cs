namespace WhereToFly.App.Model
{
    /// <summary>
    /// Parameters for the flying range calculation
    /// </summary>
    public class FlyingRangeParameters
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
    }
}
