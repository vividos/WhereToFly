using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhereToFly.Shared.Model.Serializers
{
    /// <summary>
    /// JSON converter for <see cref="DateTimeOffset"/> using ISO 8601.
    /// </summary>
    public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
    {
        /// <summary>
        /// Reads DateTimeOffset value
        /// </summary>
        /// <param name="reader">reader to use</param>
        /// <param name="typeToConvert">type to convert; unused</param>
        /// <param name="options">serializer options; unused</param>
        /// <returns>read date time object</returns>
        public override DateTimeOffset Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            Debug.Assert(
                typeToConvert == typeof(DateTimeOffset),
                "type to convert must match");

            return DateTimeOffset.ParseExact(
                reader.GetString() ?? string.Empty,
                "o",
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Writes DateTimeOffset value
        /// </summary>
        /// <param name="writer">writer to use</param>
        /// <param name="value">date time value to write</param>
        /// <param name="options">serializer options; unused</param>
        public override void Write(
            Utf8JsonWriter writer,
            DateTimeOffset value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("o"));
        }
    }
}
