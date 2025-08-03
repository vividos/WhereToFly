using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using WhereToFly.App.Abstractions;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Services
{
    /// <summary>
    /// Geographic location service
    /// </summary>
    public class GeolocationService : IGeolocationService
    {
        /// <summary>
        /// Interface to the geolocation implementation
        /// </summary>
        private readonly IGeolocation geolocation = Geolocation.Default;

        /// <summary>
        /// Returns if currently listening to position updates
        /// </summary>
        public bool IsListening => this.geolocation.IsListeningForeground;

        /// <summary>
        /// Event handler that is called for position changes
        /// </summary>
        public event EventHandler<GeolocationEventArgs>? PositionChanged;

        /// <summary>
        /// Checks for permission to use location.
        /// </summary>
        /// <returns>true when everything is ok, false when permission wasn't given</returns>
        private static async Task<bool> CheckPermissionAsync()
        {
            if (!MainThread.IsMainThread)
            {
                return await MainThread.InvokeOnMainThreadAsync(CheckPermissionAsync);
            }

            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>() &&
                    status != PermissionStatus.Granted)
                {
                    var userInterface = DependencyService.Get<IUserInterface>();

                    await userInterface.DisplayAlert(
                        "The location permission is needed in order to locate your position on the map",
                        "OK");
                }

                return status == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                App.LogError(ex);
                throw;
            }
        }

        /// <summary>
        /// Returns current position
        /// </summary>
        /// <param name="timeout">timeout for waiting for position</param>
        /// <returns>current position, or null when none could be retrieved</returns>
        public async Task<Microsoft.Maui.Devices.Sensors.Location?> GetPositionAsync(TimeSpan timeout)
        {
            if (!await CheckPermissionAsync())
            {
                return null;
            }

            try
            {
                return await Geolocation.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Default, timeout));
            }
            catch (FeatureNotEnabledException)
            {
                // Workaround until .NET 10 has Geolocation.IsEnabled
                return null;
            }
        }

        /// <summary>
        /// Returns last known position
        /// </summary>
        /// <returns>last known position, or null when none could be retrieved</returns>
        public async Task<MapPoint?> GetLastKnownPositionAsync()
        {
            if (!await CheckPermissionAsync())
            {
                return null;
            }

            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location == null)
            {
                return null;
            }

            return location.ToMapPoint();
        }

        /// <summary>
        /// Starts listening for location updates
        /// </summary>
        /// <returns>true when successful, false when not</returns>
        public async Task<bool> StartListeningAsync()
        {
            if (this.IsListening)
            {
                return true;
            }

            if (!await CheckPermissionAsync())
            {
                return false;
            }

            this.geolocation.LocationChanged += this.OnLocationChanged;
            this.geolocation.ListeningFailed += this.OnLocationError;

            return await this.geolocation.StartListeningForegroundAsync(
                new GeolocationListeningRequest(
                    Constants.GeoLocationAccuracy,
                    Constants.GeoLocationMinimumTimeForUpdate));
        }

        /// <summary>
        /// Called when the position has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs args)
        {
            if (args.Location != null)
            {
                this.PositionChanged?.Invoke(
                    sender,
                    new GeolocationEventArgs(args.Location));
            }
        }

        /// <summary>
        /// Called when listening for position resulted in an error
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnLocationError(object? sender, GeolocationListeningFailedEventArgs args)
        {
            Debug.WriteLine($"PositionError: {args.Error}");
        }

        /// <summary>
        /// Stops listening for location updates
        /// </summary>
        public void StopListening()
        {
            if (!this.IsListening)
            {
                return;
            }

            this.geolocation.LocationChanged -= this.OnLocationChanged;
            this.geolocation.ListeningFailed -= this.OnLocationError;

            this.geolocation.StopListeningForeground();
        }
    }
}
