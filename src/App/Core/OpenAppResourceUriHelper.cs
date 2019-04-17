using System;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Views;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Helper for opening AppResourceUri
    /// </summary>
    public static class OpenAppResourceUriHelper
    {
        /// <summary>
        /// Opens app resource URI, e.g. a live waypoint
        /// </summary>
        /// <param name="uri">app resource URI to open</param>
        /// <returns>task to wait on</returns>
        public static async Task Open(string uri)
        {
            try
            {
                var appResourceUri = new AppResourceUri(uri);

                if (!appResourceUri.IsValid)
                {
                    await App.Current.MainPage.DisplayAlert(
                        Constants.AppTitle,
                        "Not a valid Where-to-fly weblink: " + uri,
                        "OK");

                    return;
                }

                var rawUri = new Uri(uri);
                string waypointName = "Live Waypoint";
                if (!string.IsNullOrEmpty(rawUri.Fragment))
                {
                    waypointName =
                        System.Net.WebUtility.UrlDecode(rawUri.Fragment.TrimStart('#'));
                }

                var liveWaypoint = await GetLiveWaypointLocation(waypointName, appResourceUri);

                if (!await ShowAddLiveWaypointDialog(liveWaypoint))
                {
                    return;
                }

                await StoreLiveWaypoint(liveWaypoint);

                App.ShowToast("Live waypoint was loaded.");

                App.ZoomToLocation(liveWaypoint.MapLocation);
                App.UpdateMapLocationsList();
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while opening weblink: " + ex.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Retrieves current live waypoint map location
        /// </summary>
        /// <param name="waypointName">waypoint name to use</param>
        /// <param name="appResourceUri">app resource uri to use</param>
        /// <returns>map location, or invalid location when it couldn't be retrieved</returns>
        private static async Task<Location> GetLiveWaypointLocation(string waypointName, AppResourceUri appResourceUri)
        {
            var dataService = DependencyService.Get<IDataService>();

            LiveWaypointQueryResult result = await dataService.GetLiveWaypointDataAsync(appResourceUri.ToString());

            var mapPoint = new MapPoint(result.Data.Latitude, result.Data.Longitude, result.Data.Altitude);

            return new Location
            {
                Id = result.Data.ID + "-" + Guid.NewGuid().ToString("B"),
                Name = waypointName ?? result.Data.Name,
                MapLocation = mapPoint,
                Description = result.Data.Description.Replace("\n", "<br/>"),
                Type = LocationType.LiveWaypoint,
                InternetLink = appResourceUri.ToString()
            };
        }

        /// <summary>
        /// Shows "add live waypoint" dialog to edit live waypoint data.
        /// </summary>
        /// <param name="liveWaypoint">live waypoint</param>
        /// <returns>true when live waypoint should be added, false when not</returns>
        private static async Task<bool> ShowAddLiveWaypointDialog(Location liveWaypoint)
        {
            return await AddLiveWaypointPopupPage.ShowAsync(liveWaypoint);
        }

        /// <summary>
        /// Stores live waypoint in location list
        /// </summary>
        /// <param name="liveWaypoint">live waypoint to store in location list</param>
        /// <returns>task to wait on</returns>
        private static async Task StoreLiveWaypoint(Location liveWaypoint)
        {
            var dataService = DependencyService.Get<IDataService>();

            var locationList = await dataService.GetLocationListAsync(CancellationToken.None);
            locationList.Add(liveWaypoint);

            await dataService.StoreLocationListAsync(locationList);
        }
    }
}
