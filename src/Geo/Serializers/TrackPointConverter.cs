using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo.Serializers
{
    /// <summary>
    /// JSON converter class for track point. Track point properties are stored as JSON
    /// array, with five elements, representing the track point properties.
    /// </summary>
    public class TrackPointConverter : JsonConverter<TrackPoint>
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
        /// Reads track point from JSON
        /// </summary>
        /// <param name="reader">json reader</param>
        /// <param name="typeToConvert">object type; unused</param>
        /// <param name="options">serializer options; unused</param>
        /// <returns>created track point object, or null when reading failed</returns>
        public override TrackPoint? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            int numberIndex = 0;

            var point = new TrackPoint(0.0, 0.0, null, null);

            while (reader.Read() &&
                reader.TokenType != JsonTokenType.EndArray)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                    case JsonTokenType.EndArray:
                        // ok, let's proceed
                        break;

                    case JsonTokenType.Number:
                        double value = reader.GetDouble();
                        if (numberIndex == 0)
                        {
                            point.Latitude = value;
                        }
                        else if (numberIndex == 1)
                        {
                            point.Longitude = value;
                        }
                        else if (numberIndex == 2)
                        {
                            if ((int)value != InvalidAltitudeValue)
                            {
                                point.Altitude = value;
                            }
                        }
                        else if (numberIndex == 3)
                        {
                            if ((int)value != InvalidHeadingValue)
                            {
                                point.Heading = (int)value;
                            }
                        }
                        else if (numberIndex == 4)
                        {
                            if ((long)value != 0)
                            {
                                long unixTime = (long)value;
                                point.Time = DateTimeOffset.FromUnixTimeMilliseconds(unixTime);
                            }
                        }
                        else
                        {
                            throw new JsonException(
                                "TrackPoint number array has too many numbers (5 expected)");
                        }

                        numberIndex++;
                        break;

                    default:
                        throw new JsonException(
                            $"MapPoint object expected, but encountered {reader.TokenType}");
                }
            }

            return point;
        }

        /// <summary>
        /// Writes track point to JSON
        /// </summary>
        /// <param name="writer">json writer</param>
        /// <param name="point">track point object to write</param>
        /// <param name="options">serializer options; unused</param>
        public override void Write(
            Utf8JsonWriter writer,
            TrackPoint point,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteNumberValue(point.Latitude);
            writer.WriteNumberValue(point.Longitude);
            writer.WriteNumberValue(point.Altitude ?? InvalidAltitudeValue);
            writer.WriteNumberValue(point.Heading ?? InvalidHeadingValue);
            writer.WriteNumberValue(
                point.Time.HasValue
                ? point.Time.Value.ToUnixTimeMilliseconds()
                : 0.0);

            writer.WriteEndArray();
        }
    }
}
