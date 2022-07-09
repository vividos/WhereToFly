using System;

namespace WhereToFly.App.Core.Models
{
    /// <summary>
    /// Description for a single weather icon
    /// </summary>
    public sealed class WeatherIconDescription : IEquatable<WeatherIconDescription>
    {
        /// <summary>
        /// Name of weather icon to display
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Group of weather icon; can be used to group icons
        /// </summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// Weather icon type
        /// </summary>
        public enum IconType
        {
            /// <summary>
            /// Icon is a link to a web page
            /// </summary>
            IconLink,

            /// <summary>
            /// Icon opens another app on the device
            /// </summary>
            IconApp,

            /// <summary>
            /// Icon is a placeholder icon to add more weather icons
            /// </summary>
            IconPlaceholder,
        }

        /// <summary>
        /// Weather icon type
        /// </summary>
        public IconType Type { get; set; } = IconType.IconPlaceholder;

        /// <summary>
        /// Web link or app link
        /// </summary>
        public string WebLink { get; set; } = string.Empty;

        #region IEquatable implementation

        /// <summary>
        /// Compares this track with another
        /// </summary>
        /// <param name="other">track to compare to</param>
        /// <returns>true when track are equal, false when not</returns>
        public bool Equals(WeatherIconDescription other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Name == other.Name &&
                this.Group == other.Group &&
                this.Type == other.Type &&
                this.WebLink == other.WebLink;
        }
        #endregion

        #region object overridables implementation

        /// <summary>
        /// Compares this weather icon description to another object
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>true when weather icon descriptions are equal, false when not</returns>
        public override bool Equals(object obj) =>
            (obj is WeatherIconDescription weatherIcon) &&
            this.Equals(weatherIcon);

        /// <summary>
        /// Calculates hash code for track
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = 487;

            hashCode = (hashCode * 31) + this.Name.GetHashCode();
            hashCode = (hashCode * 31) + this.Group.GetHashCode();
            hashCode = (hashCode * 31) + this.Type.GetHashCode();
            hashCode = (hashCode * 31) + this.WebLink.GetHashCode();

            return hashCode;
        }
        #endregion
    }
}
