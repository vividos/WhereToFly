using Plugin.Geolocator.Abstractions;
using System.Threading.Tasks;
using WhereToFly.App.Geo.Spatial;

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// Geographic location service
    /// </summary>
    public class GeolocationService
    {
        /// <summary>
        /// Returns current geolocator instance
        /// </summary>
        public virtual IGeolocator Geolocator { get => Plugin.Geolocator.CrossGeolocator.Current; }

        /// <summary>
        /// Returns current position
        /// </summary>
        /// <returns>current position, or null when none could be retrieved</returns>
        public async Task<LatLongAlt> GetCurrentPositionAsync()
        {
            var position = await this.Geolocator.GetLastKnownLocationAsync();
            if (position == null)
            {
                return null;
            }

            return new LatLongAlt(position.Latitude, position.Longitude, position.Altitude);
        }
    }
}
