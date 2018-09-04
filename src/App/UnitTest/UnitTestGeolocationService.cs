using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;

// warning CS0067: The event 'UnitTestGeolocationService.UnitTestGeolocator.PositionError' is never used
#pragma warning disable 67

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Geolocation service for unit tests
    /// </summary>
    public class UnitTestGeolocationService : GeolocationService
    {
        /// <summary>
        /// IGeolocator instance for unit tests
        /// </summary>
        internal class UnitTestGeolocator : IGeolocator
        {
            /// <inheritdoc />
            public double DesiredAccuracy { get; set; } = 1.0;

            /// <inheritdoc />
            public bool IsListening { get; } = false;

            /// <inheritdoc />
            public bool SupportsHeading { get; } = false;

            /// <inheritdoc />
            public bool IsGeolocationAvailable { get; } = false;

            /// <inheritdoc />
            public bool IsGeolocationEnabled { get; } = false;

            /// <inheritdoc />
            public event EventHandler<PositionErrorEventArgs> PositionError;

            /// <inheritdoc />
            public event EventHandler<PositionEventArgs> PositionChanged;

            /// <inheritdoc />
            public Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position position, string mapKey = null)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public Task<Position> GetLastKnownLocationAsync()
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public Task<Position> GetPositionAsync(TimeSpan? timeout = null, CancellationToken? token = null, bool includeHeading = false)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address, string mapKey = null)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public Task<bool> StopListeningAsync()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns geolocator instance
        /// </summary>
        public override IGeolocator Geolocator
        {
            get => new UnitTestGeolocator();
        }
    }
}
