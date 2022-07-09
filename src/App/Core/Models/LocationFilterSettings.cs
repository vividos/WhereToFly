using System;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.Models
{
    /// <summary>
    /// Location filter settings
    /// </summary>
    public sealed class LocationFilterSettings : IEquatable<LocationFilterSettings>
    {
        /// <summary>
        /// Last used filter text
        /// </summary>
        public string FilterText { get; set; } = string.Empty;

        /// <summary>
        /// Takeoff directions to filter
        /// </summary>
        public TakeoffDirections FilterTakeoffDirections { get; set; } = TakeoffDirections.All;

        /// <summary>
        /// Filter settings that determines if non-takeoff locations should also be shown
        /// </summary>
        public bool ShowNonTakeoffLocations { get; set; } = true;

        #region object overridables implementation

        /// <summary>
        /// Returns a displayable string
        /// </summary>
        /// <returns>displayable string</returns>
        public override string ToString()
        {
            return $"Text={this.FilterText}, TakeoffDirections={this.FilterTakeoffDirections}, ShowNonTakeoffLocations={this.ShowNonTakeoffLocations}";
        }

        /// <summary>
        /// Returns hash code for app resource URI
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode() =>
            (this.FilterText,
            this.FilterTakeoffDirections,
            this.ShowNonTakeoffLocations).GetHashCode();

        /// <summary>
        /// Compares this location filter settings to another object
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>true when equal filter settings, false when not</returns>
        public override bool Equals(object obj) =>
            (obj is LocationFilterSettings filterSettings) && this.Equals(filterSettings);
        #endregion

        #region IEquatable implementation

        /// <summary>
        /// Compares this app settings to another app settings object
        /// </summary>
        /// <param name="other">app settings object to compare to</param>
        /// <returns>true when equal app settings, false when not</returns>
        public bool Equals(LocationFilterSettings other) =>
            other != null &&
            (this.FilterText,
            this.FilterTakeoffDirections,
            this.ShowNonTakeoffLocations) ==
            (other.FilterText,
            other.FilterTakeoffDirections,
            other.ShowNonTakeoffLocations);
        #endregion

        /// <summary>
        /// Equality operator
        /// </summary>
        /// <param name="left">left operator argument</param>
        /// <param name="right">right operator argument</param>
        /// <returns>true when objects are equal, false when not</returns>
        public static bool operator ==(LocationFilterSettings left, LocationFilterSettings right) =>
            Equals(left, right);

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="left">left operator argument</param>
        /// <param name="right">right operator argument</param>
        /// <returns>true when objects are inequal, false when not</returns>
        public static bool operator !=(LocationFilterSettings left, LocationFilterSettings right) =>
            !Equals(left, right);
    }
}
