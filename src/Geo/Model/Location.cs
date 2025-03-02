﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Location on map
        /// </summary>
        public MapPoint MapLocation { get; set; }

        /// <summary>
        /// Description of location
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Type of location
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter<LocationType>))]
        public LocationType Type { get; set; } = LocationType.Waypoint;

        /// <summary>
        /// When the location is of type FlyingTakeoff, this may specify the possible takeoff
        /// directions.
        /// </summary>
        public TakeoffDirections TakeoffDirections { get; set; } = TakeoffDirections.None;

        /// <summary>
        /// Link to external internet page, for more infos about location
        /// </summary>
        public string InternetLink { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if this location is a start/stop location for planning tours
        /// that is known to the backend
        /// </summary>
        public bool IsPlanTourLocation { get; set; } = false;

        /// <summary>
        /// Indicates if the location is a temporary location created for planning a tour
        /// </summary>
        public bool IsTempPlanTourLocation { get; set; } = false;

        /// <summary>
        /// Extra properties of the location, e.g. wind direction for a weather station, etc.
        /// </summary>
        public Dictionary<LocationPropertyType, string> Properties { get; set; } = [];

        /// <summary>
        /// Creates a new location object
        /// </summary>
        /// <param name="id">location ID</param>
        /// <param name="mapLocation">map location</param>
        public Location(string id, MapPoint mapLocation)
        {
            this.Id = id;
            this.MapLocation = mapLocation;
        }

        #region IEquatable implementation

        /// <summary>
        /// Compares this location with other location. Note that properties are not compared,
        /// since they might change over time when updated live.
        /// </summary>
        /// <param name="other">location to compare to first</param>
        /// <returns>true when locations are equal, false when not</returns>
        public bool Equals(Location? other)
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
        public override bool Equals(object? obj) =>
            (obj is Location location) && this.Equals(location);

        /// <summary>
        /// Calculates hash code for location
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode()
            => HashCode.Combine(
                this.Id,
                this.Name,
                this.Type,
                this.InternetLink,
                this.MapLocation,
                this.Description,
                this.IsPlanTourLocation,
                this.TakeoffDirections);

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
