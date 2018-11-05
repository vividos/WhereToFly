using Plugin.Geolocator.Abstractions;
using System.Threading.Tasks;
using WhereToFly.App.Geo.Spatial;
using WhereToFly.App.Model;

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
        public async Task<MapPoint> GetCurrentPositionAsync()
        {
            var position = await this.Geolocator.GetLastKnownLocationAsync();
            if (position == null)
            {
                return null;
            }

            return new MapPoint(position.Latitude, position.Longitude, position.Altitude);
        }
    }
}
