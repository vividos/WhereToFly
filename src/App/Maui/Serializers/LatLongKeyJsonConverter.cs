﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using WhereToFly.App.Services;

namespace WhereToFly.App.Serializers
{
    /// <summary>
    /// JSON converter class for <see cref="LatLongKey"/>.
    /// </summary>
    internal class LatLongKeyJsonConverter : JsonConverter<LatLongKey>
    {
        /// <summary>
        /// Reads lat long key from JSON
        /// </summary>
        /// <param name="reader">json reader</param>
        /// <param name="typeToConvert">object type; unused</param>
        /// <param name="options">serializer options; unused</param>
        /// <returns>lat long key, or default when reading failed</returns>
        public override LatLongKey Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                return default;
            }

            string? text = reader.GetString();
            return Parse(text);
        }

        /// <summary>
        /// Reads a lat long key as property name
        /// </summary>
        /// <param name="reader">json reader</param>
        /// <param name="typeToConvert">object type; unused</param>
        /// <param name="options">serializer options; unused</param>
        /// <returns>lat long key, or default when reading failed</returns>
        public override LatLongKey ReadAsPropertyName(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                return default;
            }

            string? text = reader.GetString();
            return Parse(text);
        }

        /// <summary>
        /// Writes lat long key to JSON
        /// </summary>
        /// <param name="writer">json writer</param>
        /// <param name="latLongKey">lat long key to write</param>
        /// <param name="options">serializer options; unused</param>
        public override void Write(
            Utf8JsonWriter writer,
            LatLongKey latLongKey,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(Format(latLongKey));
        }

        /// <summary>
        /// Writes a lat long key as property name
        /// </summary>
        /// <param name="writer">json writer</param>
        /// <param name="value">lat long key to write</param>
        /// <param name="options">serializer options; unused</param>
        public override void WriteAsPropertyName(
            Utf8JsonWriter writer,
            [DisallowNull] LatLongKey value,
            JsonSerializerOptions options)
            => writer.WritePropertyName(Format(value));

        /// <summary>
        /// Formats a lat/long key as text
        /// </summary>
        /// <returns>lat/long key as text</returns>
        private static string Format(LatLongKey latLongKey)
            => $"{latLongKey.Latitude},{latLongKey.Longitude}";

        /// <summary>
        /// Parses a lat/long key from given text
        /// </summary>
        /// <param name="text">text to parse</param>
        /// <returns>lat/long key</returns>
        private static LatLongKey Parse(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return default;
            }

            int posComma = text.IndexOf(',');
            string latitude = text[..posComma];
            string longitude = text[(posComma + 1)..];

            return new LatLongKey(
                int.Parse(latitude),
                int.Parse(longitude));
        }
    }
}
