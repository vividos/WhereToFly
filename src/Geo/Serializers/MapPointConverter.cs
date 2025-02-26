using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo.Serializers
{
    /// <summary>
    /// JSON converter class for map point. Map point properties are stored as JSON
    /// array, either with two (without altitude) or three elements (with altitude).
    /// </summary>
    public sealed class MapPointConverter : JsonConverter<MapPoint>
    {
        /// <summary>
        /// Reads map point from JSON
        /// </summary>
        /// <param name="reader">json reader</param>
        /// <param name="typeToConvert">object type; unused</param>
        /// <param name="options">serializer options; unused</param>
        /// <returns>created map point object, or null when reading failed</returns>
        public override MapPoint? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                return this.ReadLegacyJson(ref reader);
            }

            int numberIndex = 0;
            double latitude = 0.0;
            double longitude = 0.0;
            double? altitude = null;

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
                            latitude = value;
                        }
                        else if (numberIndex == 1)
                        {
                            longitude = value;
                        }
                        else if (numberIndex == 2)
                        {
                            altitude = value;
                        }
                        else
                        {
                            throw new JsonException(
                                "MapPoint number array has too many numbers (2 or 3 expected)");
                        }

                        numberIndex++;
                        break;

                    default:
                        throw new JsonException(
                            $"MapPoint object expected, but encountered {reader.TokenType}");
                }
            }

            if (numberIndex != 2 && numberIndex != 3)
            {
                return new MapPoint(0.0, 0.0);
            }
            else
            {
                return new MapPoint(latitude, longitude, altitude);
            }
        }

        /// <summary>
        /// Reads legacy JSON object as MapPoint
        /// </summary>
        /// <param name="reader">json reader</param>
        /// <returns>created map point object, or null when reading failed</returns>
        private MapPoint? ReadLegacyJson(ref Utf8JsonReader reader)
        {
            var point = new MapPoint(0.0, 0.0);
            string? nextProperty = null;

            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                        return point;

                    case JsonTokenType.PropertyName:
                        nextProperty = reader.GetString();
                        break;

                    case JsonTokenType.Number:
                        double number = reader.GetDouble();
                        switch (nextProperty)
                        {
                            case "Latitude":
                                point.Latitude = number;
                                break;

                            case "Longitude":
                                point.Longitude = number;
                                break;

                            case "Altitude":
                                point.Altitude = number;
                                break;

                            default:
                                // ignore unknown property
                                break;
                        }

                        nextProperty = null;
                        break;

                    default:
                        // ignore token
                        break;
                }
            }
            while (reader.Read());

            return point.Valid
                ? point
                : null;
        }

        /// <summary>
        /// Writes map point to JSON
        /// </summary>
        /// <param name="writer">json writer</param>
        /// <param name="point">map point object to write</param>
        /// <param name="options">serializer options; unused</param>
        public override void Write(
            Utf8JsonWriter writer,
            MapPoint point,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            if (point.Valid)
            {
                writer.WriteNumberValue(point.Latitude);
                writer.WriteNumberValue(point.Longitude);

                if (point.Altitude.HasValue)
                {
                    writer.WriteNumberValue(point.Altitude.Value);
                }
            }

            writer.WriteEndArray();
        }
    }
}
