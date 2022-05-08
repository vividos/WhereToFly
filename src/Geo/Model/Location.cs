using System;
using System.Collections.Generic;

namespace WhereToFly.Geo.Model
{
    /// <summary>
    /// A location that can be displayed on the map.
    /// </summary>
    public sealed class Location : IEquatable<Location>
    {
        /// <summary>
        /// Location ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of location
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Location on map
        /// </summary>
        public MapPoint MapLocation { get; set; }

        /// <summary>
        /// Description of location
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of location
        /// </summary>
        public LocationType Type { get; set; }

        /// <summary>
        /// When the location is of type FlyingTakeoff, this may specify the possible takeoff
        /// directions.
        /// </summary>
        public TakeoffDirections TakeoffDirections { get; set; }

        /// <summary>
        /// Link to external internet page, for more infos about location
        /// </summary>
        public string InternetLink { get; set; }

        /// <summary>
        /// Indicates if this location is a start/stop location for planning tours
        /// </summary>
        public bool IsPlanTourLocation { get; set; } = false;

        /// <summary>
        /// Extra properties of the location, e.g. wind direction for a weather station, etc.
        /// </summary>
        public Dictionary<LocationPropertyType, string> Properties { get; set; } = new();

        #region IEquatable implementation

        /// <summary>
        /// Compares this location with other location. Note that properties are not compared,
        /// since they might change over time when updated live.
        /// </summary>
        /// <param name="other">location to compare to first</param>
        /// <returns>true when locations are equal, false when not</returns>
        public bool Equals(Location other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Id == other.Id &&
                this.Name == other.Name &&
                this.Type == other.Type &&
                this.InternetLink == other.InternetLink &&
                this.MapLocation.Equals(other.MapLocation) &&
                this.Description == other.Description;
        }
        #endregion

        #region object overridables implementation

        /// <summary>
        /// Compares this location to another object
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>true when locations are equal, false when not</returns>
        public override bool Equals(object obj) =>
            (obj is Location location) && this.Equals(location);

        /// <summary>
        /// Calculates hash code for location
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = 487;

            hashCode = (hashCode * 31) + this.Id.GetHashCode();
            hashCode = (hashCode * 31) + this.Name.GetHashCode();
            hashCode = (hashCode * 31) + this.Type.GetHashCode();
            hashCode = (hashCode * 31) + this.InternetLink.GetHashCode();
            hashCode = (hashCode * 31) + this.MapLocation.GetHashCode();
            hashCode = (hashCode * 31) + this.Description.GetHashCode();
            hashCode = (hashCode * 31) + this.IsPlanTourLocation.GetHashCode();
            hashCode = (hashCode * 31) + this.TakeoffDirections.GetHashCode();

            return hashCode;
        }

        /// <summary>
        /// Returns a printable representation of this object
        /// </summary>
        /// <returns>printable text</returns>
        public override string ToString()
        {
            return $"Name={this.Name}, Type={this.Type}, MapLocation={this.MapLocation}";
        }
        #endregion
    }
}
