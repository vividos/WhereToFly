using Plugin.Geolocator.Abstractions;
using System;
using System.Threading.Tasks;
using WhereToFly.App.Core;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Geolocation service for unit tests
    /// </summary>
    public class UnitTestGeolocationService : IGeolocationService
    {
        /// <inheritdoc />
        public event EventHandler<GeolocationEventArgs> PositionChanged;

        /// <inheritdoc />
        public Task<MapPoint> GetLastKnownPositionAsync()
        {
            return Task.FromResult<MapPoint>(null);
        }

        /// <inheritdoc />
        public Task<Position> GetPositionAsync(TimeSpan timeout)
        {
            return Task.FromResult<Position>(null);
        }

        /// <inheritdoc />
        public Task<bool> StartListeningAsync()
        {
            this.PositionChanged?.Invoke(this, new GeolocationEventArgs(new Position()));
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task StopListeningAsync()
        {
            return Task.CompletedTask;
        }
    }
}
