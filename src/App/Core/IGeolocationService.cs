using System;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Interface to geolocation services
    /// </summary>
    public interface IGeolocationService
    {
        /// <summary>
        /// Event handler that is called for position changes
        /// </summary>
        event EventHandler<GeolocationEventArgs> PositionChanged;

        /// <summary>
        /// Returns current position
        /// </summary>
        /// <param name="timeout">timeout for waiting for position</param>
        /// <returns>current position, or null when none could be retrieved</returns>
        Task<Xamarin.Essentials.Location> GetPositionAsync(TimeSpan timeout);

        /// <summary>
        /// Returns last known position
        /// </summary>
        /// <returns>last known position, or null when none could be retrieved</returns>
        Task<MapPoint> GetLastKnownPositionAsync();

        /// <summary>
        /// Starts listening for location updates
        /// </summary>
        /// <returns>true when successful, false when not</returns>
        Task<bool> StartListeningAsync();

        /// <summary>
        /// Stops listening for location updates
        /// </summary>
        /// <returns>task to wait on</returns>
        Task StopListeningAsync();
    }
}
