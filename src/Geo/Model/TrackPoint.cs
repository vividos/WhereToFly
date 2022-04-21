using Newtonsoft.Json;
using System;

namespace WhereToFly.Geo.Model
{
    /// <summary>
    /// Single track point in a track
    /// </summary>
    public class TrackPoint
    {
        /// <summary>
        /// Latitude, from north (+90.0) to south (-90.0), 0.0 at equator line, e.g. 48.137155
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude, from west to east, 0.0 at Greenwich line; e.g. 11.575416
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Altitude, in meters; optional
        /// </summary>
        public double? Altitude { get; set; }

        /// <summary>
        /// Heading, in degrees; optional
        /// </summary>
        public int? Heading { get; set; }

        /// <summary>
        /// Date and time of track point
        /// </summary>
        public DateTimeOffset? Time { get; set; }

        /// <summary>
        /// Creates a new track point from given data
        /// </summary>
        /// <param name="latitude">latitude value</param>
        /// <param name="longitude">longitude value</param>
        /// <param name="altitude">altitude value; optional</param>
        /// <param name="heading">heading; optional</param>
        public TrackPoint(double latitude, double longitude, double? altitude, int? heading)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
            this.Heading = heading;
        }

        /// <summary>
        /// Nested JSON converter class for track point. Track point properties are stored as JSON
        /// array, with five elements, representing the track point properties.
        /// </summary>
        public class Converter : JsonConverter
        {
            /// <summary>
            /// Value specifying an invalid altitude value
            /// </summary>
            private const int InvalidAltitudeValue = -10000;

            /// <summary>
            /// Value specifying an invalid heading value
            /// </summary>
            private const int InvalidHeadingValue = -1;

            /// <summary>
            /// Determines if given type can be converted to a track point
            /// </summary>
            /// <param name="objectType">object type to convert to</param>
            /// <returns>true when type can be converted to, false when not</returns>
            public override bool CanConvert(Type objectType) => typeof(TrackPoint).IsAssignableFrom(objectType);

            /// <summary>
            /// Reads track point from JSON
            /// </summary>
            /// <param name="reader">json reader</param>
            /// <param name="objectType">type of object to read/return</param>
            /// <param name="existingValue">existing value; unused</param>
            /// <param name="serializer">json serializer</param>
            /// <returns>created track point object, or null when reading failed</returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var elements = serializer.Deserialize<double[]>(reader);

                if (elements == null)
                {
                    return null;
                }

                if (elements.Length != 5)
                {
                    return new TrackPoint(0.0, 0.0, null, null);
                }

                double? altitude = null;
                if ((int)elements[2] != InvalidAltitudeValue)
                {
                    altitude = elements[2];
                }

                int? heading = null;
                int convertedHeading = (int)elements[3];
                if (convertedHeading != InvalidHeadingValue)
                {
                    heading = convertedHeading;
                }

                var trackPoint = new TrackPoint(elements[0], elements[1], altitude, heading);

                if (((long)elements[4]) != 0)
                {
                    long unixTime = (long)elements[4];
                    trackPoint.Time = DateTimeOffset.FromUnixTimeMilliseconds(unixTime);
                }

                return trackPoint;
            }

            /// <summary>
            /// Writes track point to JSON
            /// </summary>
            /// <param name="writer">json writer</param>
            /// <param name="value">track point object to write</param>
            /// <param name="serializer">json serializer</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is not TrackPoint point)
                {
                    serializer.Serialize(writer, null);
                }
                else
                {
                    var array = new double[5]
                    {
                        point.Latitude,
                        point.Longitude,
                        point.Altitude ?? InvalidAltitudeValue,
                        point.Heading ?? InvalidHeadingValue,
                        point.Time.HasValue ? point.Time.Value.ToUnixTimeMilliseconds() : 0.0,
                    };

                    serializer.Serialize(writer, array);
                }
            }
        }
    }
}
