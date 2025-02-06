using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhereToFly.WebApi.Logic.Serializers
{
    /// <summary>
    /// JSON converter for DateTime that uses DateTime.Parse() instead of strict ISO 8601 parsing.
    /// </summary>
    public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
    {
        /// <summary>
        /// Reads DateTime value
        /// </summary>
        /// <param name="reader">reader to use</param>
        /// <param name="typeToConvert">type to convert; unused</param>
        /// <param name="options">serializer options; unused</param>
        /// <returns>read date time object</returns>
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime), "type to convert must match");

            return DateTime.Parse(
                reader.GetString() ?? string.Empty,
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Writes DateTime value
        /// </summary>
        /// <param name="writer">writer to use</param>
        /// <param name="value">date time value to write</param>
        /// <param name="options">serializer options; unused</param>
        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
