using Newtonsoft.Json;
using System;

namespace WhereToFly.Shared.Model
{
    /// <summary>
    /// Represents an Uniform Resource Locator (URI) for the Where-to-fly app.
    /// </summary>
    [JsonConverter(typeof(AppResourceUri.Converter))]
    public sealed class AppResourceUri : IEquatable<AppResourceUri>
    {
        /// <summary>
        /// Default URI scheme for App URIs
        /// </summary>
        public const string DefaultScheme = "where-to-fly";

        /// <summary>
        /// Stores the actual URI
        /// </summary>
        private readonly Uri uri;

        /// <summary>
        /// Resource type
        /// </summary>
        public enum ResourceType
        {
            /// <summary>
            /// No resource; invalid app resource URI
            /// </summary>
            None = -1,

            /// <summary>
            /// Uri specifies a "Find me SPOT" position; the data property contains the SPOT feed
            /// ID.
            /// </summary>
            FindMeSpotPos = 0,

            /// <summary>
            /// Specifies a "Garmin inReach" position; the data property contains the MapShare
            /// identifier.
            /// </summary>
            GarminInreachPos = 1,

            /// <summary>
            /// Specifies a position from a device in the "Open Glider Network"
            /// (https://www.glidernet.org/); the data property contains the device ID of the
            /// device.
            /// </summary>
            OpenGliderNetworkPos = 2,

            /// <summary>
            /// Live track from Garmin inReach device; the data property contains the MapShare
            /// identifier.
            /// </summary>
            GarminInreachLiveTrack = 3,

            /// <summary>
            /// Specifies a position for testing
            /// </summary>
            TestPos = 100,

            /// <summary>
            /// Live tracking test track
            /// </summary>
            TestLiveTrack = 101,
        }

        /// <summary>
        /// Property that returns if the URI is valid
        /// </summary>
        public bool IsValid
        {
            get
            {
                return this.uri != null &&
                    this.uri.Scheme == DefaultScheme &&
                    this.Type != ResourceType.None &&
                    this.Data != null;
            }
        }

        /// <summary>
        /// Property that returns if the URI is a track resource uri
        /// </summary>
        public bool IsTrackResourceType
        {
            get
            {
                return this.Type == ResourceType.TestLiveTrack ||
                    this.Type == ResourceType.GarminInreachLiveTrack;
            }
        }

        /// <summary>
        /// Resource type of the app resource URI; invalid when it contains None.
        /// </summary>
        public ResourceType Type { get; private set; } = ResourceType.None;

        /// <summary>
        /// Custom data; the meaning is specific for different resource types. This property is
        /// only valid when not null.
        /// </summary>
        public string Data { get; private set; } = null;

        /// <summary>
        /// Creates a new app resource URI object from a given URI to parse
        /// </summary>
        /// <param name="uriToParse">URI to parse</param>
        public AppResourceUri(string uriToParse)
        {
            if (uriToParse == null)
            {
                throw new ArgumentNullException(nameof(uriToParse));
            }

            this.uri = new Uri(uriToParse);
            this.ParseUri(this.uri);
        }

        /// <summary>
        /// Creates a new app resource URI object from resource type and data
        /// </summary>
        /// <param name="type">resource type to use</param>
        /// <param name="data">data to use</param>
        public AppResourceUri(ResourceType type, string data)
        {
            if (type == ResourceType.None)
            {
                throw new ArgumentException("invalid resource type", nameof(type));
            }

            this.Type = type;
            this.Data = data ?? throw new ArgumentNullException(nameof(data));

            this.uri = new Uri(this.FormatUri());
        }

        /// <summary>
        /// Parses given Uri object parts and sets Type and Data properties.
        /// </summary>
        /// <param name="uri">Uri object to parse</param>
        private void ParseUri(Uri uri)
        {
            if (!string.IsNullOrEmpty(uri.Host))
            {
                this.Type = FindResourceTypeFromString(uri.Host);
            }

            if (uri.Segments.Length == 2 &&
                uri.Segments[0] == "/")
            {
                this.Data = uri.Segments[1];
            }
        }

        /// <summary>
        /// Converts a string representation of a resource type to an enum value
        /// </summary>
        /// <param name="resourceType">resource type in all lowercase characters</param>
        /// <returns>found resource type, or None when it is unknown</returns>
        private static ResourceType FindResourceTypeFromString(string resourceType)
        {
            foreach (var item in Enum.GetValues(typeof(ResourceType)))
            {
                if (item.ToString().ToLowerInvariant() == resourceType)
                {
                    return (ResourceType)item;
                }
            }

            return ResourceType.None;
        }

        /// <summary>
        /// Returns a displayable text of the app resouce URI; this can also be used to transfer
        /// the app resource URI over the network.
        /// </summary>
        /// <returns>displayable text</returns>
        public override string ToString() => this.IsValid ? this.FormatUri() : "invalid";

        /// <summary>
        /// Formats URI from object properties, without validity checking
        /// </summary>
        /// <returns>formatted app resource URI</returns>
        private string FormatUri()
        {
            string type = this.Type.ToString().ToLowerInvariant();

            return $"{DefaultScheme}://{type}/{this.Data}";
        }

        /// <summary>
        /// Returns hash code for app resource URI
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode() => this.uri.GetHashCode();

        /// <summary>
        /// Compares this app resource URI to another object
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>true when equal URIs, false when not</returns>
        public override bool Equals(object obj) => obj is AppResourceUri other && this.Equals(other);

        /// <summary>
        /// Compares this app resource URI to another app resource URI
        /// </summary>
        /// <param name="other">other URI to compare to</param>
        /// <returns>true when equal URIs, false when not</returns>
        public bool Equals(AppResourceUri other) =>
            other != null &&
            this.uri == other.uri;

        /// <summary>
        /// Equality operator
        /// </summary>
        /// <param name="left">left operator argument</param>
        /// <param name="right">right operator argument</param>
        /// <returns>true when objects are equal, false when not</returns>
        public static bool operator ==(AppResourceUri left, AppResourceUri right) => Equals(left, right);

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="left">left operator argument</param>
        /// <param name="right">right operator argument</param>
        /// <returns>true when objects are inequal, false when not</returns>
        public static bool operator !=(AppResourceUri left, AppResourceUri right) => !Equals(left, right);

        /// <summary>
        /// Nested JSON converter class for app resource URI
        /// </summary>
        private sealed class Converter : JsonConverter
        {
            /// <summary>
            /// Determines if given type can be converted to an app resource URI
            /// </summary>
            /// <param name="objectType">object type to convert to</param>
            /// <returns>true when type can be converted to, false when not</returns>
            public override bool CanConvert(Type objectType) => typeof(AppResourceUri).IsAssignableFrom(objectType);

            /// <summary>
            /// Reads app resource URI from JSON
            /// </summary>
            /// <param name="reader">json reader</param>
            /// <param name="objectType">type of object to read/return</param>
            /// <param name="existingValue">existing value; unused</param>
            /// <param name="serializer">json serializer</param>
            /// <returns>created app resource URI object, or null when reading failed</returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                string uriAsText = serializer.Deserialize<string>(reader);
                return string.IsNullOrEmpty(uriAsText) ? null : new AppResourceUri(uriAsText);
            }

            /// <summary>
            /// Writes app resource URI to JSON
            /// </summary>
            /// <param name="writer">json writer</param>
            /// <param name="value">app resource URI object to write</param>
            /// <param name="serializer">json serializer</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var uri = value as AppResourceUri;
                writer.WriteValue(uri?.ToString() ?? string.Empty);
            }
        }
    }
}
