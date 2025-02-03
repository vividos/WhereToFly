using System.Text.Json.Serialization;
using WhereToFly.App.Serializers;

namespace WhereToFly.App.Services
{
    /// <summary>
    /// Type of key that contains a latitude and longitude value
    /// </summary>
    /// <param name="Latitude">integer latitude value</param>
    /// <param name="Longitude">integer longitude value</param>
    [JsonConverter(typeof(LatLongKeyJsonConverter))]
    internal readonly record struct LatLongKey(int Latitude, int Longitude);
}
