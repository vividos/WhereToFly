using System.ComponentModel;

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// Type of key that contains a latitude and longitude value
    /// </summary>
    [TypeConverter(typeof(LatLongKeyTypeConverter))]
    internal struct LatLongKey
    {
        /// <summary>
        /// Integer latitude value
        /// </summary>
        public int Latitude { get; set; }

        /// <summary>
        /// Integer longitude value
        /// </summary>
        public int Longitude { get; set; }

        /// <summary>
        /// Creates a new lat/long key object
        /// </summary>
        /// <param name="latitude">integer latitude value</param>
        /// <param name="longitude">integer longitude value</param>
        public LatLongKey(int latitude, int longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }
    }
}
