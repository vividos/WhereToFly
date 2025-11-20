using WhereToFly.App.Abstractions;
using WhereToFly.App.Services;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Logic
{
    /// <summary>
    /// Helper for opening <see cref="AppResourceUri"/>
    /// </summary>
    public class OpenAppResourceUriHelper
    {
        /// <summary>
        /// User interface
        /// </summary>
        private readonly IUserInterface userInterface;

        /// <summary>
        /// App map service
        /// </summary>
        private readonly IAppMapService appMapService;

        /// <summary>
        /// Data service
        /// </summary>
        private readonly IDataService dataService;

        /// <summary>
        /// Live data refresh service
        /// </summary>
        private readonly LiveDataRefreshService liveWaypointRefreshService;

        /// <summary>
        /// Creates a new helper object
        /// </summary>
        /// <param name="userInterface">user interface</param>
        /// <param name="appMapService">app map service</param>
        /// <param name="dataService">data service</param>
        /// <param name="liveWaypointRefreshService">live waypoint refresh service</param>
        public OpenAppResourceUriHelper(
            IUserInterface userInterface,
            IAppMapService appMapService,
            IDataService dataService,
            LiveDataRefreshService liveWaypointRefreshService)
        {
            this.userInterface = userInterface;
            this.appMapService = appMapService;
            this.dataService = dataService;
            this.liveWaypointRefreshService = liveWaypointRefreshService;
        }

        /// <summary>
        /// Opens app resource URI, e.g. a live waypoint
        /// </summary>
        /// <param name="uri">app resource URI to open</param>
        /// <returns>task to wait on</returns>
        public async Task OpenAsync(string uri)
        {
            try
            {
                var appResourceUri = new AppResourceUri(uri);

                if (!appResourceUri.IsValid)
                {
                    await this.userInterface.DisplayAlert(
                        "Not a valid Where-to-fly weblink: " + uri,
                        "OK");

                    return;
                }

                if (!appResourceUri.IsTrackResourceType)
                {
                    await this.AddLiveWaypoint(uri, appResourceUri);
                }
                else
                {
                    await this.AddLiveTrack(uri, appResourceUri);
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await this.userInterface.DisplayAlert(
                    "Error while opening weblink: " + ex.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Adds a new live waypoint resource URI
        /// </summary>
        /// <param name="uri">URI as string</param>
        /// <param name="appResourceUri">app resource URI</param>
        /// <returns>task to wait on</returns>
        private async Task AddLiveWaypoint(string uri, AppResourceUri appResourceUri)
        {
            var rawUri = new Uri(uri);
            string waypointName = "Live Waypoint";
            if (!string.IsNullOrEmpty(rawUri.Fragment))
            {
                waypointName =
                    System.Net.WebUtility.UrlDecode(rawUri.Fragment.TrimStart('#'));
            }

            Location? liveWaypoint = await this.GetLiveWaypointLocation(waypointName, appResourceUri);

            if (liveWaypoint == null)
            {
                return;
            }

            if (!await this.ShowAddLiveWaypointDialog(liveWaypoint))
            {
                return;
            }

            await this.StoreLiveWaypoint(liveWaypoint);

            this.userInterface.DisplayToast("Live waypoint was loaded.");

            await this.userInterface.NavigationService.GoToMap();

            await this.appMapService.UpdateLastShownPosition(liveWaypoint.MapLocation);

            this.appMapService.MapView.AddLocation(liveWaypoint);

            this.appMapService.MapView.ZoomToLocation(liveWaypoint.MapLocation);
        }

        /// <summary>
        /// Retrieves current live waypoint map location
        /// </summary>
        /// <param name="waypointName">waypoint name to use</param>
        /// <param name="appResourceUri">app resource uri to use</param>
        /// <returns>map location, or null when it couldn't be retrieved</returns>
        private async Task<Location?> GetLiveWaypointLocation(string waypointName, AppResourceUri appResourceUri)
        {
            LiveWaypointQueryResult result = await this.dataService.GetLiveWaypointDataAsync(appResourceUri.ToString());

            if (result.Data == null)
            {
                return null;
            }

            var mapPoint = new MapPoint(result.Data.Latitude, result.Data.Longitude, result.Data.Altitude);

            return new Location(
                result.Data.ID,
                mapPoint)
            {
                Name = waypointName ?? result.Data.Name,
                Description = result.Data.Description.Replace("\n", "<br/>"),
                Type = LocationType.LiveWaypoint,
                InternetLink = appResourceUri.ToString(),
            };
        }

        /// <summary>
        /// Shows "add live waypoint" dialog to edit live waypoint data.
        /// </summary>
        /// <param name="liveWaypoint">live waypoint</param>
        /// <returns>true when live waypoint should be added, false when not</returns>
        private async Task<bool> ShowAddLiveWaypointDialog(Location? liveWaypoint)
        {
            var editedLiveWaypoint = await this.userInterface.NavigationService.NavigateToPopupPageAsync<Location?>(
                PopupPageKey.AddLiveWaypointPopupPage,
                true,
                liveWaypoint);

            if (editedLiveWaypoint != null)
            {
                liveWaypoint = editedLiveWaypoint;
            }

            return liveWaypoint != null;
        }

        /// <summary>
        /// Stores live waypoint in location list
        /// </summary>
        /// <param name="liveWaypoint">live waypoint to store in location list</param>
        /// <returns>task to wait on</returns>
        private async Task StoreLiveWaypoint(Location liveWaypoint)
        {
            var locationDataService = this.dataService.GetLocationDataService();

            // remove when the live waypoint was already in the list
            await locationDataService.Remove(liveWaypoint.Id);
            await locationDataService.Add(liveWaypoint);

            this.liveWaypointRefreshService.AddLiveWaypoint(liveWaypoint);
        }

        /// <summary>
        /// Adds a new live track resource URI
        /// </summary>
        /// <param name="uri">URI as string</param>
        /// <param name="appResourceUri">app resource URI</param>
        /// <returns>task to wait on</returns>
        private async Task AddLiveTrack(string uri, AppResourceUri appResourceUri)
        {
            var rawUri = new Uri(uri);
            string trackName = "Live Track";
            if (!string.IsNullOrEmpty(rawUri.Fragment))
            {
                trackName =
                    System.Net.WebUtility.UrlDecode(rawUri.Fragment.TrimStart('#'));
            }

            Track liveTrack = await this.GetLiveTrack(trackName, appResourceUri);

            await this.StoreLiveTrack(liveTrack);

            this.userInterface.DisplayToast("Live track was loaded.");

            await this.userInterface.NavigationService.GoToMap();

            await this.appMapService.MapView.AddTrack(liveTrack);

            this.appMapService.MapView.ZoomToTrack(liveTrack);
        }

        /// <summary>
        /// Retrieves current live track points
        /// </summary>
        /// <param name="trackName">track name to use</param>
        /// <param name="appResourceUri">app resource uri to use</param>
        /// <returns>loaded live track</returns>
        private async Task<Track> GetLiveTrack(string trackName, AppResourceUri appResourceUri)
        {
            LiveTrackQueryResult result = await this.dataService.GetLiveTrackDataAsync(
                appResourceUri.ToString(),
                null);

            var track = new Track(result.Data.ID)
            {
                Name = trackName ?? result.Data.Name,
                Description = HtmlConverter.Sanitize(result.Data.Description),
                IsFlightTrack = true,
                IsLiveTrack = true,
                Color = "ff8000",
                TrackPoints = result.Data.TrackPoints.Select(
                    trackPoint => new TrackPoint(
                        latitude: trackPoint.Latitude,
                        longitude: trackPoint.Longitude,
                        altitude: trackPoint.Altitude,
                        null)
                    {
                        Time = result.Data.TrackStart.AddSeconds(trackPoint.Offset),
                    }).ToList(),
            };

            track.CalculateStatistics();

            return track;
        }

        /// <summary>
        /// Stores live track in track list
        /// </summary>
        /// <param name="liveTrack">live track to store in track list</param>
        /// <returns>task to wait on</returns>
        private async Task StoreLiveTrack(Track liveTrack)
        {
            var trackDataService = this.dataService.GetTrackDataService();

            // remove when the live track was already in the list
            try
            {
                await trackDataService.Remove(liveTrack.Id);
            }
            catch (Exception)
            {
                // ignore errors when removing
            }

            await trackDataService.Add(liveTrack);

            var liveDataRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveDataRefreshService.AddLiveTrack(liveTrack);
        }
    }
}
