using Newtonsoft.Json;
using System;
using System.Globalization;

namespace WhereToFly.Geo.Model
{
    /// <summary>
    /// A point on a map, in WGS84 decimal coordinates. Negative values are
    /// left of the GMT line and below the equator.
    /// </summary>
    [JsonConverter(typeof(MapPoint.Converter))]
    public sealed class MapPoint : IEquatable<MapPoint>
    {
        /// <summary>
        /// Creates a new map point
        /// </summary>
        /// <param name="latitude">latitude in decimal degrees</param>
        /// <param name="longitude">longitude in decimal degrees</param>
        /// <param name="altitude">altitude in meters; optional</param>
        public MapPoint(double latitude, double longitude, double? altitude = null)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
        }

        /// <summary>
        /// Latitude, from north (+90.0) to south (-90.0), 0.0 at equator line, e.g. 48.137155
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude, from west to east, 0.0 at Greenwich line; e.g. 11.575416
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Altitude above mean sea level, in meters; optional
        /// </summary>
        public double? Altitude { get; set; }

        /// <summary>
        /// Returns if map point is valid, e.g. when latitude and longitude are != 0
        /// </summary>
        [JsonIgnore]
        public bool Valid
        {
            get
            {
                return
                    Math.Abs(this.Latitude) > double.Epsilon &&
                    Math.Abs(this.Longitude) > double.Epsilon;
            }
        }

        #region IEquatable implementation

        /// <summary>
        /// Compares this map point to another map point and returns if they are equal
        /// </summary>
        /// <param name="other">other map point</param>
        /// <returns>true when equal, false when not</returns>
        public bool Equals(MapPoint other)
        {
            if (other == null)
            {
                return false;
            }

            if (this.Altitude.HasValue != other.Altitude.HasValue)
            {
                // only one has an altitude set
                return false;
            }

            bool altitudeIsEqual = true;

            if (this.Altitude.HasValue && other.Altitude.HasValue)
            {
                altitudeIsEqual = Math.Abs(this.Altitude.Value - other.Altitude.Value) < 1e-2;
            }

            return altitudeIsEqual &&
                Math.Abs(this.Latitude - other.Latitude) < 1e-6 &&
                Math.Abs(this.Longitude - other.Longitude) < 1e-6;
        }
        #endregion

        #region object overridables implementation

        /// <summary>
        /// Compares this map point to another object and returns if they are equal
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>true when equal, false when not</returns>
        public override bool Equals(object obj) =>
            obj is MapPoint other && this.Equals(other);

        /// <summary>
        /// Calculates hash code for map point
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = 487;
            hashCode = (hashCode * 31) + this.Latitude.GetHashCode();
            hashCode = (hashCode * 31) + this.Longitude.GetHashCode();
            hashCode = (hashCode * 31) + this.Altitude.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Returns a printable representation of this object
        /// </summary>
        /// <returns>printable text</returns>
        public override string ToString()
        {
            if (!this.Valid)
            {
                return "invalid";
            }

            return string.Format(
                "Lat={0}, Long={1}, Alt={2}",
                this.Latitude.ToString("F6", CultureInfo.InvariantCulture),
                this.Longitude.ToString("F6", CultureInfo.InvariantCulture),
                this.Altitude.HasValue ? this.Altitude.Value.ToString("F2", CultureInfo.InvariantCulture) : "N/A");
        }
        #endregion

        /// <summary>
        /// Nested JSON converter class for map point. Map point properties are stored as JSON
        /// array, either with two (without altitude) or three elements (with altitude).
        /// </summary>
        private sealed class Converter : JsonConverter
        {
            /// <summary>
            /// Determines if given type can be converted to a map point
            /// </summary>
            /// <param name="objectType">object type to convert to</param>
            /// <returns>true when type can be converted to, false when not</returns>
            public override bool CanConvert(Type objectType) => typeof(MapPoint).IsAssignableFrom(objectType);

            /// <summary>
            /// Reads map point from JSON
            /// </summary>
            /// <param name="reader">json reader</param>
            /// <param name="objectType">type of object to read/return</param>
            /// <param name="existingValue">existing value; unused</param>
            /// <param name="serializer">json serializer</param>
            /// <returns>created map point object, or null when reading failed</returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var elements = serializer.Deserialize<double[]>(reader);

                if (elements == null)
                {
                    return null;
                }

                if (elements.Length != 2 && elements.Length != 3)
                {
                    return new MapPoint(0.0, 0.0);
                }

                return elements.Length == 2
                    ? new MapPoint(elements[0], elements[1])
                    : new MapPoint(elements[0], elements[1], elements[2]);
            }

            /// <summary>
            /// Writes map point to JSON
            /// </summary>
            /// <param name="writer">json writer</param>
            /// <param name="value">map point object to write</param>
            /// <param name="serializer">json serializer</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is not MapPoint point)
                {
                    serializer.Serialize(writer, null);
                }
                else if (!point.Valid)
                {
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                }
                else
                {
                    var array = point.Altitude.HasValue
                        ? new double[3] { point.Latitude, point.Longitude, point.Altitude.Value }
                        : new double[2] { point.Latitude, point.Longitude };

                    serializer.Serialize(writer, array);
                }
            }
        }
    }
}
