﻿using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using WhereToFly.App.Behaviors;
using WhereToFly.App.MapView;
using WhereToFly.App.Models;
using WhereToFly.App.Services;
using WhereToFly.App.ViewModels;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;
using Location = WhereToFly.Geo.Model.Location;

namespace WhereToFly.App.Pages
{
    /// <summary>
    /// Page showing a map, with pins for locations loaded into the app.
    /// </summary>
    public class MapPage : ContentPage
    {
        /// <summary>
        /// App map service
        /// </summary>
        private readonly IAppMapService appMapService;

        /// <summary>
        /// Data service
        /// </summary>
        private readonly IDataService dataService;

        /// <summary>
        /// Geolocation service to use for position updates
        /// </summary>
        private readonly IGeolocationService geolocationService;

        /// <summary>
        /// Tour planning parameters for the map page
        /// </summary>
        private readonly PlanTourParameters planTourParameters = new();

        /// <summary>
        /// Map view control on C# side
        /// </summary>
        private readonly MapView.MapView mapView;

        /// <summary>
        /// Map page view model
        /// </summary>
        private readonly MapPageViewModel viewModel;

        /// <summary>
        /// Toolbar button for planning tour
        /// </summary>
        private ToolbarItem? planTourToolbarButton;

        /// <summary>
        /// Indicates if the next position update should also zoom to my position
        /// </summary>
        private bool zoomToMyPosition;

        /// <summary>
        /// Current app settings object
        /// </summary>
        private AppSettings? appSettings;

        /// <summary>
        /// Current app config object
        /// </summary>
        private AppConfig? appConfig;

        /// <summary>
        /// List of locations on the map
        /// </summary>
        private List<Location>? locationList;

        /// <summary>
        /// List of tracks on the map
        /// </summary>
        private List<Track>? trackList;

        /// <summary>
        /// List of layers on the map
        /// </summary>
        private List<Layer>? layerList;

        /// <summary>
        /// Access to the map view instance
        /// </summary>
        internal IMapView MapView => this.mapView
            ?? throw new InvalidOperationException("accessing MapView before it is initialized");

        /// <summary>
        /// Creates a new maps page
        /// </summary>
        public MapPage()
        {
            this.appMapService = DependencyService.Get<IAppMapService>();
            this.dataService = DependencyService.Get<IDataService>();

            this.Title = Constants.AppTitle;
            this.BackgroundColor = Colors.Black;

            this.zoomToMyPosition = false;

            this.geolocationService = DependencyService.Get<IGeolocationService>();

#if ANDROID || WINDOWS
            string cacheFolder = FileSystem.CacheDirectory;
#else
            string cacheFolder = System.AppContext.BaseDirectory;
#endif

            var nearbyPoiService = new NearbyPoiCachingService(
                new BackendDataService(),
                cacheFolder);

            this.mapView = new MapView.MapView
            {
                LogErrorAction = App.LogError,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                AutomationId = "ExploreMapWebView",
                NearbyPoiService = nearbyPoiService,
            };

            this.viewModel = new MapPageViewModel(
                this.appMapService,
                this.geolocationService);

#if WINDOWS
            var dropRecognizer = new DropGestureRecognizer
            {
                AllowDrop = true,
            };

            dropRecognizer.Drop +=
                async (sender, args) =>
                {
                    args.Handled = true;
                    await App.OnDropFile(args);
                };

            this.mapView.GestureRecognizers.Add(dropRecognizer);
#endif

            this.BindingContext = this.viewModel;

            this.Dispatcher.DispatchAsync(this.InitLayoutAsync)
                .LogTaskException();
        }

        /// <summary>
        /// Initializes layout by loading map html into web view
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task InitLayoutAsync()
        {
            this.SetupToolbar();

            this.SetupWebView();

            await this.LoadDataAsync();

            await this.CreateMapViewAsync();
        }

        /// <summary>
        /// Sets up toolbar for this page
        /// </summary>
        private void SetupToolbar()
        {
            this.AddPlanTourButton();
            this.AddFindNearbyPoisButton();
            this.AddLocateMeToolbarButton();
            this.AddFindLocationToolbarButton();
        }

        /// <summary>
        /// Initializes the "plan tour" button that is initially invisible
        /// </summary>
        private void AddPlanTourButton()
        {
            this.planTourToolbarButton = new ToolbarItem(
                "Plan tour",
                "map_marker_plus.png",
                async () => await this.ShowPlanTourPopup(),
                ToolbarItemOrder.Primary,
                0)
            {
                AutomationId = "PlanTour",
            };
        }

        /// <summary>
        /// Updates visibility of "plan tour" button
        /// </summary>
        /// <param name="isVisible">
        /// true to show the button, or false to hide it
        /// </param>
        private void UpdatePlanTourButtonVisibility(bool isVisible)
        {
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(
                    () => this.UpdatePlanTourButtonVisibility(isVisible));
                return;
            }

            if (this.planTourToolbarButton == null)
            {
                return;
            }

            if (isVisible &&
                !this.ToolbarItems.Contains(this.planTourToolbarButton))
            {
                this.ToolbarItems.Add(this.planTourToolbarButton);
            }
            else if (!isVisible &&
                this.ToolbarItems.Contains(this.planTourToolbarButton))
            {
                this.ToolbarItems.Remove(this.planTourToolbarButton);
            }
        }

        /// <summary>
        /// Adds a "find nearby pois" button to the toolbar
        /// </summary>
        private void AddFindNearbyPoisButton()
        {
            var findNearbyPoisButton = new ToolbarItem(
                "Find nearby POIs",
                "magnify_scan.png",
                async () => await this.OnClicked_ToolbarButtonFindNearbyPois(),
                ToolbarItemOrder.Primary,
                1)
            {
                AutomationId = "FindNearbyPois",
            };

            this.ToolbarItems.Add(findNearbyPoisButton);
        }

        /// <summary>
        /// Adds a "locate me" button to the toolbar
        /// </summary>
        private void AddLocateMeToolbarButton()
        {
            var locateMeButton = new ToolbarItem(
                "Locate me",
                "crosshairs_gps.png",
                async () => await this.OnClicked_ToolbarButtonLocateMe(),
                ToolbarItemOrder.Primary,
                2)
            {
                AutomationId = "LocateMe",
            };

            this.ToolbarItems.Add(locateMeButton);
        }

        /// <summary>
        /// Called when toolbar button "Find nearby pois" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonFindNearbyPois()
        {
            try
            {
                await this.MapView.FindNearbyPois();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("exception while finding nearby POIs: " + ex.ToString());

                var userInterface = DependencyService.Get<IUserInterface>();

                await userInterface.DisplayAlert(
                    "Error while finding nearby POIs: " + ex.Message,
                    "Close");
            }
        }

        /// <summary>
        /// Called when toolbar button "Locate me" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonLocateMe()
        {
            Microsoft.Maui.Devices.Sensors.Location? position;
            try
            {
                position = await this.geolocationService.GetPositionAsync(
                    timeout: TimeSpan.FromMilliseconds(100));
            }
            catch (Exception ex)
            {
                // no position service activated, or timeout reached
                App.LogError(ex);

                // zoom at next update
                this.zoomToMyPosition = true;

                return;
            }

            if (position != null &&
                Math.Abs(position.Latitude) < 1e5 &&
                Math.Abs(position.Longitude) < 1e5 &&
                this.mapView != null)
            {
                var point = position.ToMapPoint();

                await this.appMapService.UpdateLastShownPosition(point);

                this.mapView.UpdateMyLocation(
                    point,
                    (int)(position.Accuracy ?? 10000),
                    (position.Speed ?? 0.0) * Geo.Constants.FactorMeterPerSecondToKilometerPerHour,
                    position.Timestamp,
                    zoomToLocation: true);
            }
            else
            {
                // zoom at next update
                this.zoomToMyPosition = true;
            }

            // also start listening to location updates
            await this.geolocationService.StartListeningAsync();
        }

        /// <summary>
        /// Adds "Find location" toolbar button
        /// </summary>
        private void AddFindLocationToolbarButton()
        {
            var currentPositionDetailsButton = new ToolbarItem(
                "Find location",
                "magnify.png",
                async () => await this.OnClicked_ToolbarButtonFindLocation(),
                ToolbarItemOrder.Primary,
                3)
            {
                AutomationId = "FindLocation",
            };

            this.ToolbarItems.Add(currentPositionDetailsButton);
        }

        /// <summary>
        /// Called when toolbar button "Find location" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonFindLocation()
        {
            string? text = await NavigationService.Instance.NavigateToPopupPageAsync<string>(
                PopupPageKey.FindLocationPopupPage,
                true);

            if (text == null)
            {
                // user cancelled the popup
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                var userInterface = DependencyService.Get<IUserInterface>();

                await userInterface.DisplayAlert(
                    "No location text was entered",
                    "Cancel");

                return;
            }

            if (CoordinatesParser.TryParse(text, out var parsedCoordinates) &&
                parsedCoordinates != null)
            {
                this.mapView.ShowFindResult(text, parsedCoordinates);
                return;
            }

            IEnumerable<Microsoft.Maui.Devices.Sensors.Location>? foundLocationsList = null;

            try
            {
                foundLocationsList = await Geocoding.GetLocationsAsync(text);
            }
            catch (Exception ex)
            {
                // ignore exceptions and just log; assume that no location was found
                App.LogError(ex);
            }

            if (foundLocationsList == null ||
                !foundLocationsList.Any())
            {
                var userInterface = DependencyService.Get<IUserInterface>();

                await userInterface.DisplayAlert(
                    "The location could not be found",
                    "OK");

                return;
            }

            var location = foundLocationsList.First();
            var point = location.ToMapPoint();

            this.mapView.ShowFindResult(text, point);
        }

        /// <summary>
        /// Sets up WebView control. Loads the map html page into the web view, creates the C#
        /// MapView instance.
        /// </summary>
        private void SetupWebView()
        {
            this.mapView.Behaviors.Add(
                new OpenLinkExternalBrowserWebViewBehavior());

            this.mapView.ShowLocationDetails += async (locationId) => await this.OnMapView_ShowLocationDetails(locationId);
            this.mapView.NavigateToLocation += async (locationId) => await this.OnMapView_NavigateToLocation(locationId);
            this.mapView.ShareMyLocation += () => this.OnMapView_ShareMyLocation();
            this.mapView.AddFindResult += async (name, point) => await this.OnMapView_AddFindResult(name, point);
            this.mapView.LongTap += async (point) => await this.OnMapView_LongTap(point);
            this.mapView.AddTourPlanLocation += async (locationId) => await this.OnMapView_AddTourPlanLocation(locationId);
            this.mapView.AddTempTourPlanPoint += async (point) => await this.AddTempPlanTourPoint(point);
            this.mapView.UpdateLastShownLocation += async (point, viewingDistance)
                => await this.OnMapView_UpdateLastShownLocation(point, viewingDistance);
            this.mapView.SetLocationAsCompassTarget += async (locationId)
                => await this.OnMapView_SetLocationAsCompassTarget(locationId);
            this.mapView.ShowTrackDetails += async (locationId) => await this.OnMapView_ShowTrackDetails(locationId);

            // UWP needs to create the renderer in the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                this.Content = this.mapView;
            });
        }

        /// <summary>
        /// Loads data
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task LoadDataAsync()
        {
            this.appSettings = await this.dataService.GetAppSettingsAsync(CancellationToken.None);
            this.appConfig = await this.dataService.GetAppConfigAsync(CancellationToken.None);
            this.locationList = (await this.dataService.GetLocationDataService().GetList()).ToList();
            this.trackList = (await this.dataService.GetTrackDataService().GetList()).ToList();
            this.layerList = (await this.dataService.GetLayerDataService().GetList()).ToList();
        }

        /// <summary>
        /// Creates the map view. Uses the C# MapView object to create its JavaScript MapView
        /// counterpart. Also locations, tracks and layers are added. The call doesn't wait for
        /// the map view to finish initialization. MapView would have a task to wait for that
        /// event, though.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task CreateMapViewAsync()
        {
            if (this.appSettings == null ||
                this.locationList == null ||
                this.trackList == null ||
                this.layerList == null)
            {
                Debug.Assert(false, "must load data before calling CreateMapViewAsync()!");
                return;
            }

            MapPoint initialCenter = this.appSettings.LastShownPosition ?? Constants.InitialCenterPoint;
            if (!initialCenter.Valid)
            {
                initialCenter = Constants.InitialCenterPoint;
            }

            int lastViewingDistance = this.appSettings.LastViewingDistance;
            if (lastViewingDistance == 0)
            {
                lastViewingDistance = 5000;
            }

            string? cesiumIonApiKey = null;
            string? bingMapsApiKey = null;

            if (this.appConfig != null)
            {
                this.appConfig.ApiKeys.TryGetValue("CesiumIonApiKey", out cesiumIonApiKey);
                this.appConfig.ApiKeys.TryGetValue("BingMapsApiKey", out bingMapsApiKey);
            }

            await this.mapView.CreateAsync(
                initialCenter,
                lastViewingDistance,
                this.appSettings.UseMapEntityClustering,
                cesiumIonApiKey,
                bingMapsApiKey);

            this.mapView.MapImageryType = this.appSettings.MapImageryType;
            this.mapView.MapOverlayType = this.appSettings.MapOverlayType;
            this.mapView.MapShadingMode = this.appSettings.ShadingMode;
            this.mapView.CoordinateDisplayFormat = this.appSettings.CoordinateDisplayFormat;

            this.mapView.ShowMessageBand("Loading locations...");
            await this.mapView.AddLocationList(this.locationList);
            this.mapView.HideMessageBand();

            this.mapView.ShowMessageBand("Loading tracks...");

            foreach (var track in this.trackList)
            {
                await this.mapView.AddTrack(track);
            }

            this.trackList.Clear();

            this.mapView.ShowMessageBand("Loading layer...");

            foreach (var layer in this.layerList)
            {
                await this.mapView.AddLayer(layer);
            }

            this.layerList.Clear();

            this.mapView.HideMessageBand();

            var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveWaypointRefreshService.UpdateLiveData += this.OnUpdateLiveData;
        }

        /// <summary>
        /// Called when live waypoint or live track location has been updated
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnUpdateLiveData(object? sender, LiveDataUpdateEventArgs args)
        {
            LiveWaypointData? waypointData = args.WaypointData;
            if (waypointData != null)
            {
                Location? location = this.FindLocationById(waypointData.ID);
                if (location != null)
                {
                    location.MapLocation = new MapPoint(waypointData.Latitude, waypointData.Longitude, waypointData.Altitude);
                    location.Description = waypointData.Description;
                    location.Name = waypointData.Name;

                    this.MapView.UpdateLocation(location);

                    var locationDataService = this.dataService.GetLocationDataService();

                    Task.Run(async () => await locationDataService.Update(location));
                }
            }

            if (args.TrackData != null)
            {
                var trackDataService = this.dataService.GetTrackDataService();

                Task.Run(async () =>
                {
                    var track = await trackDataService.Get(args.TrackData.ID);

                    if (track == null)
                    {
                        return;
                    }

                    track.Name = args.TrackData.Name;
                    track.Description = args.TrackData.Description;
                    MergeTrackPoints(track, args.TrackData);

                    track.CalculateStatistics();

                    await trackDataService.Update(track);
                });

                this.mapView.UpdateLiveTrack(args.TrackData);
            }
        }

        /// <summary>
        /// Merges track points from live track data with the given track
        /// </summary>
        /// <param name="track">track to merge new track points to</param>
        /// <param name="trackData">newly received track data</param>
        private static void MergeTrackPoints(Track track, LiveTrackData trackData)
        {
            var trackPoints = trackData.TrackPoints.Select(
                    trackPoint => new TrackPoint(
                        latitude: trackPoint.Latitude,
                        longitude: trackPoint.Longitude,
                        altitude: trackPoint.Altitude,
                        null)
                    {
                        Time = trackData.TrackStart.AddSeconds(trackPoint.Offset),
                    }).ToList();

            if (trackPoints.Count != 0)
            {
                DateTimeOffset? trackDataStart = trackPoints[0].Time;

                if (trackDataStart.HasValue)
                {
                    track.RemoveTrackPointsAfter(trackDataStart.Value);
                    track.TrackPoints.AddRange(trackPoints);
                }
            }
        }

        /// <summary>
        /// Called when user clicked on the "Show details" link in the pin description on the
        /// map. Starts the location details page.
        /// </summary>
        /// <param name="locationId">location id of location to show details for</param>
        /// <returns>task to wait on</returns>
        private async Task OnMapView_ShowLocationDetails(string locationId)
        {
            Location? location = this.FindLocationById(locationId);

            if (location == null)
            {
                Debug.WriteLine("couldn't find location with id=" + locationId);
                return;
            }

            await NavigationService.Instance.NavigateAsync(
                PageKey.LocationDetailsPage,
                animated: true,
                parameter: location);
        }

        /// <summary>
        /// Called when user clicked on the "Navigate here" link in the pin description on the
        /// map. Starts route navigation to location.
        /// </summary>
        /// <param name="locationId">location id of location to navigate to</param>
        /// <returns>task to wait on</returns>
        private async Task OnMapView_NavigateToLocation(string locationId)
        {
            Location? location = this.FindLocationById(locationId);

            if (location == null)
            {
                Debug.WriteLine("couldn't find location with id=" + locationId);
                return;
            }

            await NavigateToPointAsync(location.Name, location.MapLocation);
        }

        /// <summary>
        /// Starts route navigation to given named point
        /// </summary>
        /// <param name="name">name of point on map to navigate to; may be empty</param>
        /// <param name="point">map point to navigate to</param>
        /// <returns>task to wait on</returns>
        private static async Task NavigateToPointAsync(string name, MapPoint point)
        {
            var navigateLocation = new Microsoft.Maui.Devices.Sensors.Location(
                latitude: point.Latitude,
                longitude: point.Longitude);

            var options = new MapLaunchOptions
            {
                Name = name,
                NavigationMode = NavigationMode.Driving,
            };

            await Map.OpenAsync(navigateLocation, options);
        }

        /// <summary>
        /// Finds a location by given location id.
        /// </summary>
        /// <param name="locationId">location id to use</param>
        /// <returns>found location, or null when no location could be found</returns>
        private Location? FindLocationById(string locationId)
        {
            Location? foundLocation = null;

            if (this.locationList != null)
            {
                foundLocation =
                    this.locationList.Find(location => location.Id == locationId);
            }

            if (foundLocation == null)
            {
                foundLocation =
                    this.mapView.GetNearbyPoiLocationById(locationId);
            }

            return foundLocation;
        }

        /// <summary>
        /// Called when the user clicked on the "Share position" link in the "my position" pin description.
        /// </summary>
        private void OnMapView_ShareMyLocation()
        {
            this.viewModel.ShareMyLocationCommand.Execute(null);
        }

        /// <summary>
        /// Called when the user clicked on the "Add find result" link in the "find result" pin
        /// description.
        /// </summary>
        /// <param name="name">name of find result to add</param>
        /// <param name="point">map point of find result</param>
        /// <returns>task to wait on</returns>
        private async Task OnMapView_AddFindResult(string name, MapPoint point)
        {
            if (this.locationList == null)
            {
                Debug.Assert(false, "calling OnMapView_AddFindResult() before data was loaded!");
                return;
            }

            var location = new Location(
                Guid.NewGuid().ToString("B"),
                point)
            {
                Name = name ?? "Unknown",
                Description = string.Empty,
                Type = LocationType.Waypoint,
                InternetLink = string.Empty,
            };

            this.locationList.Add(location);

            var locationDataService = this.dataService.GetLocationDataService();

            await locationDataService.Add(location);

            await NavigationService.Instance.NavigateAsync(
                PageKey.EditLocationDetailsPage,
                animated: true,
                parameter: location);

            this.mapView.AddLocation(location);
        }

        /// <summary>
        /// Called when the last shown location should be updated in the app settings.
        /// </summary>
        /// <param name="point">map point to store</param>
        /// <param name="viewingDistance">current viewing distance</param>
        /// <returns>task to wait on</returns>
        private async Task OnMapView_UpdateLastShownLocation(MapPoint point, int viewingDistance)
        {
            await this.appMapService.UpdateLastShownPosition(point, viewingDistance);
        }

        /// <summary>
        /// Called when a location has been set as the compass target.
        /// </summary>
        /// <param name="locationId">location ID of new target location; may be null</param>
        /// <returns>task to wait on</returns>
        private async Task OnMapView_SetLocationAsCompassTarget(string locationId)
        {
            if (string.IsNullOrEmpty(locationId) ||
                locationId == "null")
            {
                await this.appMapService.SetCompassTarget(null);
                return;
            }

            Location? location = this.FindLocationById(locationId);

            if (location == null)
            {
                Debug.WriteLine("couldn't set location as compass target, with id=" + locationId);
                return;
            }

            var compassTarget = new CompassTarget
            {
                Title = location.Name,
                TargetLocation = location.MapLocation,
            };

            await this.appMapService.SetCompassTarget(compassTarget);
        }

        /// <summary>
        /// Called when user clicked on the "Show info" button in the height profile view. Starts
        /// the track details page.
        /// </summary>
        /// <param name="trackId">track ID of track to show details for</param>
        /// <returns>task to wait on</returns>
        private async Task OnMapView_ShowTrackDetails(string trackId)
        {
            // since trackList is cleared for memory savings, ask the track data service directly
            Track? track = await this.dataService.GetTrackDataService().Get(trackId);

            if (track == null)
            {
                Debug.WriteLine("couldn't find track with id=" + trackId);
                return;
            }

            await NavigationService.Instance.NavigateAsync(
                PageKey.TrackInfoPage,
                animated: true,
                parameter: track);
        }

        /// <summary>
        /// Called when the user performed a long-tap on the map
        /// </summary>
        /// <param name="point">map point where long-tap occured</param>
        /// <returns>task to wait on</returns>
        private async Task OnMapView_LongTap(MapPoint point)
        {
            if (this.appSettings == null)
            {
                Debug.Assert(false, "calling OnMapView_LongTap() before data was loaded!");
                return;
            }

            var result = await MapLongTapContextMenu.ShowAsync(point, this.appSettings);

            switch (result)
            {
                case MapLongTapContextMenu.Result.AddNewWaypoint:
                    await this.AddNewWaypoint(point);
                    break;

                case MapLongTapContextMenu.Result.SetAsCompassTarget:
                    await this.SetAsCompassTarget(point);
                    break;

                case MapLongTapContextMenu.Result.NavigateHere:
                    await NavigateToPointAsync(string.Empty, point);
                    break;

                case MapLongTapContextMenu.Result.ShowFlyingRange:
                    await this.ShowFlyingRange(point);
                    break;

                case MapLongTapContextMenu.Result.PlanTour:
                    await this.AddTempPlanTourPoint(point);
                    break;

                case MapLongTapContextMenu.Result.FindFlights:
                    await this.viewModel.FindFlights(point);
                    break;

                case MapLongTapContextMenu.Result.Cancel:
                    // ignore
                    break;

                default:
                    Debug.Assert(false, "invalid context menu result");
                    break;
            }
        }

        /// <summary>
        /// Adds new waypoint with given map point
        /// </summary>
        /// <param name="point">map point</param>
        /// <returns>task to wait on</returns>
        public async Task AddNewWaypoint(MapPoint point)
        {
            if (this.locationList == null)
            {
                Debug.Assert(false, "calling AddNewWaypoint() before data was loaded!");
                return;
            }

            var location = new Location(
                Guid.NewGuid().ToString("B"),
                point)
            {
                Name = string.Empty,
                Description = string.Empty,
                Type = LocationType.Waypoint,
                InternetLink = string.Empty,
            };

            await NavigationService.Instance.NavigateAsync(
                PageKey.EditLocationDetailsPage,
                animated: true,
                parameter: location);

            this.locationList.Add(location);

            var locationDataService = this.dataService.GetLocationDataService();

            await locationDataService.Add(location);

            TakeoffDirectionsHelper.AddTakeoffDirection(location);

            this.mapView.AddLocation(location);
        }

        /// <summary>
        /// Sets map point as compass target
        /// </summary>
        /// <param name="point">map point to set as target</param>
        /// <returns>task to wait on</returns>
        private async Task SetAsCompassTarget(MapPoint point)
        {
            var compassTarget = new CompassTarget
            {
                Title = "Selected location",
                TargetLocation = point,
            };

            await this.appMapService.SetCompassTarget(compassTarget);
        }

        /// <summary>
        /// Shows flying range from given point; shows a half transparent cone on the map that is
        /// created with some flying parameters like gliding ratio, wind spee and direction.
        /// </summary>
        /// <param name="point">point to calculate flying range for</param>
        /// <returns>task to wait on</returns>
        private async Task ShowFlyingRange(MapPoint point)
        {
            var parameters = await NavigationService.Instance.NavigateToPopupPageAsync<FlyingRangeParameters>(
                PopupPageKey.FlyingRangePopupPage,
                true);

            if (parameters != null)
            {
                this.mapView.ShowFlyingRange(point, parameters);
            }
        }

        /// <summary>
        /// Called when user clicked on the "Plan tour" link in the pin description on the
        /// map. Shows popup dialog for tour planning.
        /// </summary>
        /// <param name="locationId">location id of location to add to tour planning</param>
        /// <returns>task to wait on</returns>
        private async Task OnMapView_AddTourPlanLocation(string locationId)
        {
            Location? location = this.FindLocationById(locationId);

            if (location == null)
            {
                Debug.WriteLine("couldn't add location with id=" + locationId);
                return;
            }

            await this.AddTourPlanningLocationAsync(location);
        }

        /// <summary>
        /// Called to add a location to tour planning
        /// </summary>
        /// <param name="location">location to add</param>
        /// <returns>task to wait on</returns>
        public async Task AddTourPlanningLocationAsync(Location location)
        {
            this.planTourParameters.WaypointIdList.Add(location.Id);

            this.UpdatePlanTourButtonVisibility(true);

            await this.ShowPlanTourPopup();
        }

        /// <summary>
        /// Shows the "plan tour" popup
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ShowPlanTourPopup()
        {
            await NavigationService.Instance.NavigateToPopupPageAsync<object>(
                PopupPageKey.PlanTourPopupPage,
                true,
                this.planTourParameters);

            this.UpdatePlanTourButtonVisibility(
                this.planTourParameters.WaypointIdList.Any());
        }

        /// <summary>
        /// Adds a new temporary plan tour point by coordinates
        /// </summary>
        /// <param name="point">map point</param>
        /// <returns>task to wait on</returns>
        private async Task AddTempPlanTourPoint(MapPoint point)
        {
            if (this.locationList == null)
            {
                Debug.Assert(false, "calling AddTempPlanTourPoint() before data was loaded!");
                return;
            }

            CoordinateDisplayFormat format =
                this.appSettings?.CoordinateDisplayFormat
                ?? CoordinateDisplayFormat.Format_dd_dddddd;

            string name =
                "At " +
                GeoDataFormatter.FormatLatLong(point.Latitude, format) +
                ", " +
                GeoDataFormatter.FormatLatLong(point.Longitude, format);

            var planTourLocation = new Location(
                Guid.NewGuid().ToString("B"),
                point)
            {
                Name = name,
                Type = LocationType.Waypoint,
                IsPlanTourLocation = false,
                IsTempPlanTourLocation = true,
            };

            this.locationList.Add(planTourLocation);

            var locationDataService = this.dataService.GetLocationDataService();

            await locationDataService.Add(planTourLocation);

            await this.AddTourPlanningLocationAsync(planTourLocation);
        }

        /// <summary>
        /// Clears all temporary plan tour locations after planning tour
        /// </summary>
        /// <returns>task to wait on</returns>
        internal async Task ClearTempPlanTourLocations()
        {
            var tempLocationsList = this.planTourParameters.WaypointLocationList
                .Where(location => location.IsTempPlanTourLocation);

            var locationDataService = this.dataService.GetLocationDataService();

            foreach (var tempLocation in tempLocationsList)
            {
                this.appMapService.MapView.RemoveLocation(tempLocation.Id);

                this.locationList?.Remove(tempLocation);

                await locationDataService.Remove(tempLocation.Id);
            }

            this.UpdatePlanTourButtonVisibility(false);
        }

        #region Page lifecycle methods
        /// <summary>
        /// Called when page is appearing; start position updates
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            this.geolocationService.PositionChanged += this.OnPositionChanged;

            Connectivity.ConnectivityChanged += this.OnConnectivityChanged;
        }

        /// <summary>
        /// Called when network connectivity of the device has changed. Sends a notification to
        /// the map view.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs args)
        {
            bool isConnectivityAvailable =
                args.NetworkAccess == NetworkAccess.Internet;

            this.mapView.OnNetworkConnectivityChanged(isConnectivityAvailable);
        }

        /// <summary>
        /// Reloads map view app settings
        /// </summary>
        public void ReloadMapViewAppSettings()
        {
            this.appSettings = App.Settings;

            if (this.appSettings == null)
            {
                Debug.Assert(false, "calling ReloadMapViewAppSettings() before data was loaded!");
                return;
            }

            this.mapView.MapImageryType = this.appSettings.MapImageryType;
            this.mapView.MapOverlayType = this.appSettings.MapOverlayType;
            this.mapView.MapShadingMode = this.appSettings.ShadingMode;
            this.mapView.CoordinateDisplayFormat = this.appSettings.CoordinateDisplayFormat;
            this.mapView.UseEntityClustering = this.appSettings.UseMapEntityClustering;
        }

        /// <summary>
        /// Called when form is disappearing; stop position updates
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            this.geolocationService.PositionChanged -= this.OnPositionChanged;
            Connectivity.ConnectivityChanged -= this.OnConnectivityChanged;
        }
        #endregion

        /// <summary>
        /// Called when position has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args, including position</param>
        private void OnPositionChanged(object? sender, GeolocationEventArgs args)
        {
            MapPoint point = args.Point;

            bool zoomToPosition = this.zoomToMyPosition;

            this.zoomToMyPosition = false;

            this.mapView?.UpdateMyLocation(
                point,
                (int)(args.Position.Accuracy ?? 10000),
                (args.Position.Speed ?? 0.0) * Geo.Constants.FactorMeterPerSecondToKilometerPerHour,
                args.Position.Timestamp,
                zoomToPosition);

            if (zoomToPosition)
            {
                Task.Run(async () => await this.appMapService.UpdateLastShownPosition(point));
            }
        }
    }
}
