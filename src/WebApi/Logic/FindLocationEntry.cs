﻿using SQLite;
using System;
using System.Text.Json;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model.Serializers;

namespace WhereToFly.WebApi.Logic
{
    /// <summary>
    /// Database entry for a location
    /// </summary>
    [Table("locations")]
    internal class FindLocationEntry
    {
        /// <summary>
        /// Location to store in the entry
        /// </summary>
        [Ignore]
        public Location Location { get; set; }

        /// <summary>
        /// Location ID
        /// </summary>
        [Column("id"), PrimaryKey]
        public string Id
        {
            get => this.Location.Id;
            set => this.Location.Id = value;
        }

        /// <summary>
        /// Latitude of location
        /// </summary>
        [Column("latitude"), Indexed]
        public double Latitude
        {
            get => this.Location.MapLocation?.Latitude ?? 0.0;
            set => this.Location.MapLocation = new MapPoint(latitude: value, longitude: this.Longitude);
        }

        /// <summary>
        /// Longitude of location
        /// </summary>
        [Column("longitude"), Indexed]
        public double Longitude
        {
            get => this.Location.MapLocation?.Longitude ?? 0.0;
            set => this.Location.MapLocation = new MapPoint(latitude: this.Latitude, longitude: value);
        }

        /// <summary>
        /// Location as JSON
        /// </summary>
        [Column("json")]
        public string Json
        {
            get => JsonSerializer.Serialize(
                this.Location,
                SharedModelJsonSerializerContext.Default.Location);
            set => this.Location =
                JsonSerializer.Deserialize(
                    value,
                    SharedModelJsonSerializerContext.Default.Location)
                ?? throw new FormatException("invalid location JSON");
        }

        /// <summary>
        /// Creates an empty location entry; used when loading entry from database
        /// </summary>
        public FindLocationEntry()
        {
            this.Location = new Location(
                Guid.NewGuid().ToString("B"),
                new MapPoint(0.0, 0.0));
        }

        /// <summary>
        /// Creates a new entry from given location
        /// </summary>
        /// <param name="location">location to use</param>
        public FindLocationEntry(Location location)
        {
            this.Location = location;
        }
    }
}
