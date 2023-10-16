using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Views;
using WhereToFly.App.MapView;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using Xamarin.Essentials;
using Xamarin.Forms;
using Location = WhereToFly.Geo.Model.Location;

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// The app's map services implementation
    /// </summary>
    internal class AppMapService : IAppMapService
    {
        /// <summary>
        /// Application settings; this is always available, since AppMapService is created after
        /// loading app settings
        /// </summary>
        private AppSettings Settings => App.Settings!;

        /// <summary>
        /// The one and only map page (displaying the map using CesiumJS)
        /// </summary>
        public MapPage MapPage => (Application.Current as App)?.MapPage
            ?? throw new InvalidOperationException("accessing MapPage before it is initialized");

        /// <summary>
        /// Access to the map view instance
        /// </summary>
        public IMapView MapView => this.MapPage.MapView;

        /// <summary>
        /// Adds a tour planning location to the current list of locations and opens the planning
        /// dialog.
        /// </summary>
        /// <param name="location">location to add</param>
        /// <returns>task to wait on</returns>
        public async Task AddTourPlanLocation(Location location)
        {
            if (this.MapPage != null)
            {
                await this.MapPage.AddTourPlanningLocationAsync(location);
            }
        }

        /// <summary>
        /// Adds track to map view
        /// </summary>
        /// <param name="track">track to add</param>
        /// <returns>task to wait on</returns>
        public async Task AddTrack(Track track)
        {
            var point = track.CalculateCenterPoint();
            await this.UpdateLastShownPosition(point);

            await this.MapView.AddTrack(track);
        }

        /// <summary>
        /// Updates map settings on opened MapPage.
        /// </summary>
        public void UpdateMapSettings()
        {
            this.MapPage?.ReloadMapViewAppSettings();
        }

        /// <summary>
        /// Updates last shown position in app settings
        /// </summary>
        /// <param name="point">current position</param>
        /// <param name="viewingDistance">current viewing distance; may be unset</param>
        /// <returns>task to wait on</returns>
        public async Task UpdateLastShownPosition(MapPoint point, int? viewingDistance = null)
        {
            if (this.Settings == null)
            {
                return; // app settings not loaded yet
            }

            if (point.Valid)
            {
                this.Settings.LastShownPosition = point;

                if (viewingDistance.HasValue)
                {
                    this.Settings.LastViewingDistance = viewingDistance.Value;
                }

                var dataService = DependencyService.Get<IDataService>();
                await dataService.StoreAppSettingsAsync(this.Settings);
            }
        }

        /// <summary>
        /// Sets (or clears) the current compass target
        /// </summary>
        /// <param name="compassTarget">compass target; may be null</param>
        /// <returns>task to wait on</returns>
        public async Task SetCompassTarget(CompassTarget? compassTarget)
        {
            if (this.Settings == null)
            {
                return; // app settings not loaded yet
            }

            this.Settings.CurrentCompassTarget = compassTarget;

            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreAppSettingsAsync(this.Settings);

            if (compassTarget == null)
            {
                this.MapView.ClearCompass();
            }
            else
            {
                if (compassTarget.TargetLocation != null)
                {
                    this.MapView.SetCompassTarget(
                        compassTarget.Title,
                        compassTarget.TargetLocation,
                        zoomToPolyline: true);
                }
                else
                {
                    Debug.Assert(
                        compassTarget.TargetDirection.HasValue,
                        "either target location or target direction must be set");

                    this.MapView.SetCompassDirection(
                        compassTarget.Title,
                        compassTarget.TargetDirection ?? 0);
                }
            }
        }

        /// <summary>
        /// Opens app resource URI, e.g. a live waypoint
        /// </summary>
        /// <param name="uri">app resource URI to open</param>
        public void OpenAppResourceUri(string uri)
        {
            MainThread.BeginInvokeOnMainThread(
                async () => await OpenAppResourceUriHelper.OpenAsync(uri));
        }

        /// <summary>
        /// Initializes live waypoint refresh service with current location list
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task InitLiveWaypointRefreshService()
        {
            var dataService = DependencyService.Get<IDataService>();
            var locationDataService = dataService.GetLocationDataService();

            var locationList = await locationDataService.GetList();

            var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveWaypointRefreshService.DataService = dataService;

            liveWaypointRefreshService.AddLiveWaypointList(locationList);

            var trackDataService = dataService.GetTrackDataService();

            var trackList = await trackDataService.GetList();

            var liveTrackList = trackList.Where(track => track.IsLiveTrack);

            foreach (var liveTrack in liveTrackList)
            {
                liveWaypointRefreshService.AddLiveTrack(liveTrack);
            }
        }

        /// <summary>
        /// Shows the flight planning disclaimer dialog, when not already shown to the user.
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ShowFlightPlanningDisclaimer()
        {
            var dataService = DependencyService.Get<IDataService>();
            var appSettings = await dataService.GetAppSettingsAsync(CancellationToken.None);

            if (appSettings.ShownFlightPlanningDisclaimer)
            {
                return;
            }

            await Device.InvokeOnMainThreadAsync(async () =>
            {
                const string DisclaimerMessage =
                    "The display and use of flight maps and airspace data can contain errors " +
                    "and their use does not release the pilot from the legal obligation of " +
                    "thorough and orderly preflight planning, nor from the use of all required " +
                    "and approved means of navigation (e.g. Aeronautical Chart ICAO 1:500,000).";

                await this.MapPage.DisplayAlert(
                    Constants.AppTitle,
                    DisclaimerMessage,
                    "Understood");
            });

            appSettings.ShownFlightPlanningDisclaimer = true;
            await dataService.StoreAppSettingsAsync(appSettings);
        }
    }
}
