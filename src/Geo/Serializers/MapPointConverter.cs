using Newtonsoft.Json;
using System;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo.Serializers
{
    /// <summary>
    /// JSON converter class for map point. Map point properties are stored as JSON
    /// array, either with two (without altitude) or three elements (with altitude).
    /// </summary>
    public sealed class MapPointConverter : JsonConverter
    {
        /// <summary>
        /// Determines if given type can be converted to a map point
        /// </summary>
        /// <param name="objectType">object type to convert to</param>
        /// <returns>true when type can be converted to, false when not</returns>
        public override bool CanConvert(Type objectType) =>
            typeof(MapPoint).IsAssignableFrom(objectType);

        /// <summary>
        /// Reads map point from JSON
        /// </summary>
        /// <param name="reader">json reader</param>
        /// <param name="objectType">type of object to read/return</param>
        /// <param name="existingValue">existing value; unused</param>
        /// <param name="serializer">json serializer</param>
        /// <returns>created map point object, or null when reading failed</returns>
        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {
            double[]? elements = serializer.Deserialize<double[]>(reader);

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
        public override void WriteJson(
            JsonWriter writer,
            object? value,
            JsonSerializer serializer)
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
                double[] array = point.Altitude.HasValue
                    ? [point.Latitude, point.Longitude, point.Altitude.Value]
                    : [point.Latitude, point.Longitude];

                serializer.Serialize(writer, array);
            }
        }
    }
}
