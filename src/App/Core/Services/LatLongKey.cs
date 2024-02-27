using System.ComponentModel;

namespace WhereToFly.App.Services
{
    /// <summary>
    /// Type of key that contains a latitude and longitude value
    /// </summary>
    /// <param name="Latitude">integer latitude value</param>
    /// <param name="Longitude">integer longitude value</param>
    [TypeConverter(typeof(LatLongKeyTypeConverter))]
    internal readonly record struct LatLongKey(int Latitude, int Longitude);
}
