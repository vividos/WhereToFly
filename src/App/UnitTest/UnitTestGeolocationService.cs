using System;
using System.Threading.Tasks;
using WhereToFly.App.Core;
using WhereToFly.Shared.Model;
using Xamarin.Essentials;

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
        public Task<Location> GetPositionAsync(TimeSpan timeout)
        {
            return Task.FromResult<Location>(null);
        }

        /// <inheritdoc />
        public Task<bool> StartListeningAsync()
        {
            this.PositionChanged?.Invoke(this, new GeolocationEventArgs(new Location()));
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task StopListeningAsync()
        {
            return Task.CompletedTask;
        }
    }
}
