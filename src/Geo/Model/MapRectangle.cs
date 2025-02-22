﻿using System;
using System.Text.Json.Serialization;

namespace WhereToFly.Geo.Model
{
    /// <summary>
    /// Map rectangle
    /// </summary>
    public record MapRectangle
    {
        /// <summary>
        /// Map point containing the north-west point of the rectangle
        /// </summary>
        [JsonIgnore]
        public MapPoint NorthWest { get; set; }

        /// <summary>
        /// Map point containing the south-east point of the rectangle
        /// </summary>
        [JsonIgnore]
        public MapPoint SouthEast { get; set; }

        /// <summary>
        /// Returns the north latitude value
        /// </summary>
        public double North
        {
            get => this.NorthWest.Latitude;
            set => this.NorthWest.Latitude = value;
        }

        /// <summary>
        /// Returns the west longitude value
        /// </summary>
        public double West
        {
            get => this.NorthWest.Longitude;
            set => this.NorthWest.Longitude = value;
        }

        /// <summary>
        /// Returns the south latitude value
        /// </summary>
        public double South
        {
            get => this.SouthEast.Latitude;
            set => this.SouthEast.Latitude = value;
        }

        /// <summary>
        /// Returns the east longitude value
        /// </summary>
        public double East
        {
            get => this.SouthEast.Longitude;
            set => this.SouthEast.Longitude = value;
        }

        /// <summary>
        /// Width of rectangle, in degrees longitude
        /// </summary>
        [JsonIgnore]
        public double Width => Math.Abs(this.East - this.West);

        /// <summary>
        /// Height of rectangle, in degrees latitude
        /// </summary>
        [JsonIgnore]
        public double Height => Math.Abs(this.North - this.South);

        /// <summary>
        /// Returns the center point of the map rectangle
        /// </summary>
        [JsonIgnore]
        public MapPoint Center => new(
            this.South + (this.Height / 2.0),
            this.West + (this.Width / 2.0));

        /// <summary>
        /// Returns if the rectangle is value
        /// </summary>
        [JsonIgnore]
        public bool Valid => this.NorthWest.Valid && this.SouthEast.Valid;

        /// <summary>
        /// Creates a new, empty, invalid map rectangle
        /// </summary>
        public MapRectangle()
            : this(0.0, 0.0, 0.0, 0.0)
        {
        }

        /// <summary>
        /// Creates a new map rectangle object
        /// </summary>
        /// <param name="north">north latitude value</param>
        /// <param name="east">east longitude value</param>
        /// <param name="south">south latitude value</param>
        /// <param name="west">west longitude value</param>
        public MapRectangle(double north, double east, double south, double west)
        {
            this.NorthWest = new MapPoint(north, west);
            this.SouthEast = new MapPoint(south, east);
        }

        /// <summary>
        /// Determines if a map point with given latitude and longitude is inside the rectangle.
        /// </summary>
        /// <param name="latitude">latitude of point</param>
        /// <param name="longitude">longitude of point</param>
        /// <returns>true when point is inside rectangle, or false when not</returns>
        public bool IsInside(double latitude, double longitude)
        {
            return
                this.South < latitude && latitude < this.North &&
                this.West < longitude && longitude < this.East;
        }
    }
}
