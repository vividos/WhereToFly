using System;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Parameters for the flying range calculation
    /// </summary>
    public sealed class FlyingRangeParameters : IEquatable<FlyingRangeParameters>
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

        #region object overridables implementation

        /// <summary>
        /// Returns hash code for app resource URI
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode() =>
            (this.GlideRatio,
            this.GliderSpeed,
            this.WindSpeed,
            this.WindDirection).GetHashCode();

        /// <summary>
        /// Compares this app settings to another object
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>true when equal app settings, false when not</returns>
        public override bool Equals(object obj) =>
            (obj is FlyingRangeParameters flyingRangeParameters) && this.Equals(flyingRangeParameters);
        #endregion

        #region IEquatable implementation

        /// <summary>
        /// Compares this app settings to another app settings object
        /// </summary>
        /// <param name="other">flying range parameters object to compare to</param>
        /// <returns>true when equal app settings, false when not</returns>
        public bool Equals(FlyingRangeParameters other) =>
            other != null &&
            (this.GlideRatio,
            this.GliderSpeed,
            this.WindSpeed,
            this.WindDirection) ==
            (other.GlideRatio,
            other.GliderSpeed,
            other.WindSpeed,
            other.WindDirection);
        #endregion

        /// <summary>
        /// Equality operator
        /// </summary>
        /// <param name="left">left operator argument</param>
        /// <param name="right">right operator argument</param>
        /// <returns>true when objects are equal, false when not</returns>
        public static bool operator ==(FlyingRangeParameters left, FlyingRangeParameters right) =>
            Equals(left, right);

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="left">left operator argument</param>
        /// <param name="right">right operator argument</param>
        /// <returns>true when objects are inequal, false when not</returns>
        public static bool operator !=(FlyingRangeParameters left, FlyingRangeParameters right) =>
            !Equals(left, right);
    }
}
