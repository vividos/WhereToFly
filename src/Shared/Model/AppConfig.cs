using System;
using System.Collections.Generic;

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
        public DateTimeOffset ExpiryDate { get; set; }
    }
}
