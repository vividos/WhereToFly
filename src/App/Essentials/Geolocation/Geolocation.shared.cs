using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace WhereToFly.App.Essentials
{
    public static partial class Geolocation
    {
        public static bool IsListening => PlatformIsListening();

        public static Task<bool> StartListeningForegroundAsync(GeolocationRequest request) =>
            PlatformStartListeningForegroundAsync(request);

        public static Task<bool> StopListeningForegroundAsync() =>
            PlatformStopListeningForegroundAsync();

        public static event EventHandler<LocationEventArgs> LocationChanged;

        internal static void OnLocationChanged(Location location) =>
            OnLocationChanged(new LocationEventArgs(location));

        internal static void OnLocationChanged(LocationEventArgs e) =>
            LocationChanged?.Invoke(null, e);

        public class LocationEventArgs : EventArgs
        {
            public Location Location { get; }

            public LocationEventArgs(Location location)
            {
                if (location == null)
                {
                    throw new ArgumentNullException(nameof(location));
                }

                this.Location = location;
            }
        }
    }
}
