using System;

namespace WhereToFly.Shared.Model
{
    /// <summary>
    /// Configuration data for the WhereToFly app
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// API key for Cesium ION
        /// </summary>
        public string CesiumIonApiKey { get; set; }

        /// <summary>
        /// API key for Bing maps
        /// </summary>
        public string BingMapsApiKey { get; set; }

        /// <summary>
        /// Date/time when validity of infos expire
        /// </summary>
        public DateTimeOffset ExpiryDate { get; set; }
    }
}
