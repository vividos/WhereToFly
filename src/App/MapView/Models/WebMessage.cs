using System.Text.Json.Serialization;

namespace WhereToFly.App.MapView.Models
{
    /// <summary>
    /// Web message and data
    /// </summary>
    internal class WebMessage
    {
        /// <summary>
        /// Message keyword
        /// </summary>
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Message data
        /// </summary>
        [JsonPropertyName("data")]
        public string? Data { get; set; }
    }
}
