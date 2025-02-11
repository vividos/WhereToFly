using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WhereToFly.Shared.Model.Serializers;

namespace WhereToFly.Shared.Model
{
    /// <summary>
    /// Configuration data for the WhereToFly app
    /// </summary>
    public record AppConfig
    {
        /// <summary>
        /// Mapping of API key names to values
        /// </summary>
        public Dictionary<string, string> ApiKeys { get; set; } = [];

        /// <summary>
        /// Date/time when validity of infos expire
        /// </summary>
        [JsonConverter(typeof(DateTimeOffsetJsonConverter))]
        public DateTimeOffset ExpiryDate { get; set; }
    }
}
