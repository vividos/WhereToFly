using Plugin.Geolocator.Abstractions;
using System;
using System.Threading.Tasks;
using WhereToFly.Shared.Model;
using Xamarin.Essentials;

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
        /// Checks for permission to use location.
        /// </summary>
        /// <returns>true when everything is ok, false when permission wasn't given</returns>
        public static async Task<bool> CheckPermissionAsync()
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
                    await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(
                        Constants.AppTitle,
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
