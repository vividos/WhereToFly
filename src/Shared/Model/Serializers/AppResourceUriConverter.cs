using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhereToFly.Shared.Model.Serializers
{
    /// <summary>
    /// JSON converter class for app resource URI.
    /// </summary>
    public sealed class AppResourceUriConverter : JsonConverter<AppResourceUri>
    {
        /// <summary>
        /// Reads app resource URI from JSON
        /// </summary>
        /// <param name="reader">json reader</param>
        /// <param name="typeToConvert">object type; unused</param>
        /// <param name="options">serializer options; unused</param>
        /// <returns>created app resource URI object, or null when reading failed</returns>
        public override AppResourceUri? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                return null;
            }

            string? uriAsText = reader.GetString();

            return !string.IsNullOrEmpty(uriAsText)
                ? new AppResourceUri(uriAsText)
                : null;
        }

        /// <summary>
        /// Writes app resource URI to JSON
        /// </summary>
        /// <param name="writer">json writer</param>
        /// <param name="appResourceUri">app resource URI object to write</param>
        /// <param name="options">serializer options; unused</param>
        public override void Write(
            Utf8JsonWriter writer,
            AppResourceUri appResourceUri,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(
                appResourceUri?.ToString());
        }
    }
}
