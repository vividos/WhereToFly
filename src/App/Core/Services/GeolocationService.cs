using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Threading.Tasks;
using WhereToFly.Shared.Model;

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
        /// Checks for permission to use geolocator. See
        /// https://github.com/jamesmontemagno/PermissionsPlugin
        /// </summary>
        /// <returns>true when everything is ok, false when permission wasn't given</returns>
        public static async Task<bool> CheckPermissionAsync()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);

                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                    {
                        await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(
                            Constants.AppTitle,
                            "The location permission is needed in order to locate your position on the map",
                            "OK");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Location });

                    status = results[Permission.Location];
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
