using System;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.Models
{
    /// <summary>
    /// Specifies the current compass target, either a fixed location or a direction angle from
    /// current location. Note that target location overrides the target direction, if both are
    /// set.
    /// </summary>
    public sealed class CompassTarget : IEquatable<CompassTarget>
    {
        /// <summary>
        /// Title of compass target
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Target location map point
        /// </summary>
        public MapPoint TargetLocation { get; set; }

        /// <summary>
        /// Target direction angle, in degrees
        /// </summary>
        public int? TargetDirection { get; set; }

        #region object overridables implementation

        /// <summary>
        /// Returns hash code for this object
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode() =>
            (this.Title,
            this.TargetLocation,
            this.TargetDirection).GetHashCode();

        /// <summary>
        /// Compares this compass target to another object
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>true when equal compass targets, false when not</returns>
        public override bool Equals(object obj) =>
            (obj is CompassTarget compassTarget) && this.Equals(compassTarget);
        #endregion

        #region IEquatable implementation

        /// <summary>
        /// Compares this compass target to another compass target object
        /// </summary>
        /// <param name="other">compass target object to compare to</param>
        /// <returns>true when equal compass targets, false when not</returns>
        public bool Equals(CompassTarget other) =>
            other != null &&
            (this.Title,
            this.TargetLocation,
            this.TargetDirection) ==
            (other.Title,
            other.TargetLocation,
            other.TargetDirection);
        #endregion

        /// <summary>
        /// Equality operator
        /// </summary>
        /// <param name="left">left operator argument</param>
        /// <param name="right">right operator argument</param>
        /// <returns>true when objects are equal, false when not</returns>
        public static bool operator ==(CompassTarget left, CompassTarget right) =>
            Equals(left, right);

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="left">left operator argument</param>
        /// <param name="right">right operator argument</param>
        /// <returns>true when objects are inequal, false when not</returns>
        public static bool operator !=(CompassTarget left, CompassTarget right) =>
            !Equals(left, right);
    }
}
