using System;
using System.Threading.Tasks;
using WhereToFly.App.Abstractions;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Geolocation service for unit tests
    /// </summary>
    public class UnitTestGeolocationService : IGeolocationService
    {
        /// <inheritdoc />
        public bool IsListening => true;

        /// <inheritdoc />
        public event EventHandler<GeolocationEventArgs>? PositionChanged;

        /// <inheritdoc />
        public Task<MapPoint?> GetLastKnownPositionAsync()
        {
            return Task.FromResult<MapPoint?>(null);
        }

        /// <inheritdoc />
        public Task<Microsoft.Maui.Devices.Sensors.Location?> GetPositionAsync(TimeSpan timeout)
        {
            return Task.FromResult<Microsoft.Maui.Devices.Sensors.Location?>(null);
        }

        /// <inheritdoc />
        public Task<bool> StartListeningAsync()
        {
            this.PositionChanged?.Invoke(
                this,
                new GeolocationEventArgs(
                    new Microsoft.Maui.Devices.Sensors.Location()));

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public void StopListening()
        {
            // do nothing
        }
    }
}
