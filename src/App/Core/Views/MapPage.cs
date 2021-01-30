using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Geo;
using WhereToFly.App.Logic;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page showing a map, with pins for locations loaded into the app.
    /// </summary>
    public class MapPage : ContentPage
    {
        /// <summary>
        /// Task completion source to signal that the web page has been loaded
        /// </summary>
        private readonly TaskCompletionSource<bool> taskCompletionSourcePageLoaded
            = new TaskCompletionSource<bool>();

        /// <summary>
        /// Geolocation service to use for position updates
        /// </summary>
        private readonly IGeolocationService geolocationService;

        /// <summary>
        /// List of track IDs currently being displayed
        /// </summary>
        private readonly HashSet<Track> displayedTracks = new HashSet<Track>();

        /// <summary>
        /// Tour planning parameters for the map page
        /// </summary>
        private readonly PlanTourParameters planTourParameters = new PlanTourParameters();

        /// <summary>
        /// List of location IDs currently being displayed
        /// </summary>
        private HashSet<string> displayedLocationIds = new HashSet<string>();

        /// <summary>
        /// Indicates if the next position update should also zoom to my position
        /// </summary>
        private bool zoomToMyPosition;

        /// <summary>
        /// Map view control on C# side
        /// </summary>
        private MapView mapView;

        /// <summary>
        /// Current app settings object
        /// </summary>
        private AppSettings appSettings;

        /// <summary>
        /// List of locations on the map
        /// </summary>
        private List<Location> locationList;

        /// <summary>
        /// List of tracks on the map
        /// </summary>
        private List<Track> trackList;

        /// <summary>
        /// List of layers on the map
        /// </summary>
        private List<Layer> layerList;

        /// <summary>
        /// Access to the map view instance
        /// </summary>
        internal IMapView MapView => this.mapView;

        /// <summary>
        /// Creates a new maps page
        /// </summary>
        public MapPage()
        {
            this.Title = Constants.AppTitle;

            this.zoomToMyPosition = false;

            this.geolocationService = DependencyService.Get<IGeolocationService>();

            Task.Run(this.InitLayoutAsync);

            MessagingCenter.Subscribe<App, Location>(
                this,
                Constants.MessageAddTourPlanLocation,
                async (app, location) => await this.AddTourPlanningLocationAsync(location));

            MessagingCenter.Subscribe<App>(this, Constants.MessageUpdateMapSettings, (app) => this.OnMessageUpdateMapSettings());
            MessagingCenter.Subscribe<App>(this, Constants.MessageUpdateMapLocations, (app) => this.OnMessageUpdateMapLocations());
            MessagingCenter.Subscribe<App>(this, Constants.MessageUpdateMapTracks, (app) => this.OnMessageUpdateMapTracks());
        }

        /// <summary>
        /// Initializes layout by loading map html into web view
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task InitLayoutAsync()
        {
            App.RunOnUiThread(() => this.SetupToolbar());

            await this.SetupWebViewAsync();

            await this.LoadDataAsync();

            await this.CreateMapViewAsync();
        }

        /// <summary>
        /// Reloads map by re-creating map view
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ReloadMapAsync()
        {
            await this.CreateMapViewAsync();
        }

        /// <summary>
        /// Sets up toolbar for this page
        /// </summary>
        private void SetupToolbar()
        {
            this.AddLocateMeToolbarButton();
            this.AddFindLocationToolbarButton();
        }

        /// <summary>
        /// Adds a "locate me" button to the toolbar
        /// </summary>
        private void AddLocateMeToolbarButton()
        {
            ToolbarItem locateMeButton = new ToolbarItem(
                "Locate me",
                Converter.ImagePathConverter.GetDeviceDependentImage("crosshairs_gps"),
                async () => await this.OnClicked_ToolbarButtonLocateMe(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "LocateMe"
            };

            this.ToolbarItems.Add(locateMeButton);
        }

        /// <summary>
        /// Called when toolbar button "Locate me" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonLocateMe()
        {
            Plugin.Geolocator.Abstractions.Position position;
            try
            {
                position = await this.geolocationService.GetPositionAsync(timeout: TimeSpan.FromMilliseconds(100));
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
                var point = new MapPoint(position.Latitude, position.Longitude, position.Altitude);

                await App.UpdateLastShownPositionAsync(point);

                this.mapView.UpdateMyLocation(
                    point,
                    (int)position.Accuracy,
                    position.Speed * Geo.Spatial.Constants.FactorMeterPerSecondToKilometerPerHour,
                    position.Timestamp,
                    zoomToLocation: true);
            }
            else
            {
                // zoom at next update
                this.zoomToMyPosition = true;
            }
        }

        /// <summary>
        /// Adds track to map view
        /// </summary>
        /// <param name="track">track to add</param>
        public void AddTrack(Track track)
        {
            this.MapView.AddTrack(track);
            this.displayedTracks.Add(track);
        }

        /// <summary>
        /// Adds "Find location" toolbar button
        /// </summary>
        private void AddFindLocationToolbarButton()
        {
            ToolbarItem currentPositionDetailsButton = new ToolbarItem(
                "Find location",
                Converter.ImagePathConverter.GetDeviceDependentImage("magnify"),
                async () => await this.OnClicked_ToolbarButtonFindLocation(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "FindLocation"
            };

            this.ToolbarItems.Add(currentPositionDetailsButton);
        }

        /// <summary>
        /// Called when toolbar button "Find location" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonFindLocation()
        {
            string text = await FindLocationPopupPage.ShowAsync();

            if (string.IsNullOrWhiteSpace(text))
            {
                await this.DisplayAlert(
                    Constants.AppTitle,
                    "No location text was entered",
                    "Cancel");

                return;
            }

            IEnumerable<Xamarin.Essentials.Location> foundLocationsList = null;

            try
            {
                foundLocationsList = await Xamarin.Essentials.Geocoding.GetLocationsAsync(text);
            }
            catch (Exception ex)
            {
                // ignore exceptions and just log; assume that no location was found
                App.LogError(ex);
            }

            if (foundLocationsList == null ||
                !foundLocationsList.Any())
            {
                await this.DisplayAlert(
                    Constants.AppTitle,
                    "The location could not be found",
                    "OK");

                return;
            }

            var location = foundLocationsList.First();
            var point = new MapPoint(location.Latitude, location.Longitude, location.Altitude);

            this.mapView.ShowFindResult(text, point);
        }

        /// <summary>
        /// Sets up WebView control. Loads the map html page into the web view, creates the C#
        /// MapView instance and waits for the page to load.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task SetupWebViewAsync()
        {
            var platform = DependencyService.Get<IPlatform>();

            WebViewSource webViewSource = null;
            if (Device.RuntimePlatform == Device.Android ||
                Device.RuntimePlatform == Device.iOS)
            {
                string htmlText = platform.LoadAssetText("map/map3D.html");

                webViewSource = new HtmlWebViewSource
                {
                    Html = htmlText,
                    BaseUrl = platform.WebViewBasePath + "map/"
                };
            }

            if (Device.RuntimePlatform == Device.UWP)
            {
                webViewSource = new UrlWebViewSource
                {
                    Url = platform.WebViewBasePath + "map/map3D.html"
                };
            }

            var webView = new WebView
            {
                Source = webViewSource,

                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            webView.AutomationId = "ExploreMapWebView";

            webView.Navigating += this.OnNavigating_WebView;
            webView.Navigated += this.OnNavigated_WebView;

            this.mapView = new MapView(webView);

            this.mapView.ShowLocationDetails += async (locationId) => await this.OnMapView_ShowLocationDetails(locationId);
            this.mapView.NavigateToLocation += async (locationId) => await this.OnMapView_NavigateToLocation(locationId);
            this.mapView.ShareMyLocation += async () => await this.OnMapView_ShareMyLocation();
            this.mapView.AddFindResult += async (name, point) => await this.OnMapView_AddFindResult(name, point);
            this.mapView.LongTap += async (point) => await this.OnMapView_LongTap(point);
            this.mapView.AddTourPlanLocation += async (locationId) => await this.OnMapView_AddTourPlanLocation(locationId);
            this.mapView.UpdateLastShownLocation += async (point, viewingDistance)
                => await this.OnMapView_UpdateLastShownLocation(point, viewingDistance);

            this.Content = webView;

            await this.taskCompletionSourcePageLoaded.Task;
        }

        /// <summary>
        /// Called when web view navigates to a new URL
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigating_WebView(object sender, WebNavigatingEventArgs args)
        {
            if (args.NavigationEvent == WebNavigationEvent.NewPage &&
                args.Url.StartsWith("http"))
            {
                Xamarin.Essentials.Launcher.OpenAsync(args.Url);
                args.Cancel = true;
            }
        }

        /// <summary>
        /// Called when navigation to current page has finished
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigated_WebView(object sender, WebNavigatedEventArgs args)
        {
            if (!this.taskCompletionSourcePageLoaded.Task.IsCompleted)
            {
                this.taskCompletionSourcePageLoaded.SetResult(true);
            }
        }

        /// <summary>
        /// Loads data; async method
        /// </summary>
        /// <returns>app info object</returns>
        private async Task LoadDataAsync()
        {
            var dataService = DependencyService.Get<IDataService>();

            this.appSettings = await dataService.GetAppSettingsAsync(CancellationToken.None);
            this.locationList = (await dataService.GetLocationDataService().GetList()).ToList();
            this.trackList = (await dataService.GetTrackDataService().GetList()).ToList();
            this.layerList = (await dataService.GetLayerDataService().GetList()).ToList();
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

            await this.mapView.CreateAsync(
                initialCenter,
                lastViewingDistance,
                this.appSettings.UseMapEntityClustering);

            this.mapView.MapImageryType = this.appSettings.MapImageryType;
            this.mapView.MapOverlayType = this.appSettings.MapOverlayType;
            this.mapView.MapShadingMode = this.appSettings.ShadingMode;
            this.mapView.CoordinateDisplayFormat = this.appSettings.CoordinateDisplayFormat;

            this.mapView.AddLocationList(this.locationList);
            this.displayedLocationIds = new HashSet<string>(
                this.locationList.Select(location => location.Id));

            foreach (var track in this.trackList)
            {
                this.AddTrack(track);
            }

            foreach (var layer in this.layerList)
            {
                this.mapView.AddLayer(layer);
            }

            var liveWaypointRefreshService = DependencyService.Get<LiveWaypointRefreshService>();
            liveWaypointRefreshService.UpdateLiveWaypoint += this.OnUpdateLiveWaypoint;
        }

        /// <summary>
        /// Called when live waypoint location has been updated
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnUpdateLiveWaypoint(object sender, LiveWaypointUpdateEventArgs args)
        {
            var location = this.FindLocationById(args.Data.ID);
            if (location != null)
            {
                location.MapLocation = new MapPoint(args.Data.Latitude, args.Data.Longitude, args.Data.Altitude);
                location.Description = args.Data.Description;
                location.Name = args.Data.Name;

                this.mapView.UpdateLocation(location);

                var dataService = DependencyService.Get<IDataService>();
                var locationDataService = dataService.GetLocationDataService();

                Task.Run(async () => await locationDataService.Update(location));
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
            Location location = this.FindLocationById(locationId);

            if (location == null)
            {
                Debug.WriteLine("couldn't find location with id=" + locationId);
                return;
            }

            await NavigationService.Instance.NavigateAsync(
                Constants.PageKeyLocationDetailsPage,
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
            Location location = this.FindLocationById(locationId);

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
            var navigateLocation = new Xamarin.Essentials.Location(
                latitude: point.Latitude,
                longitude: point.Longitude);

            var options = new Xamarin.Essentials.MapLaunchOptions
            {
                Name = name,
                NavigationMode = Xamarin.Essentials.NavigationMode.Driving,
            };

            await Xamarin.Essentials.Map.OpenAsync(navigateLocation, options);
        }

        /// <summary>
        /// Finds a location by given location id.
        /// </summary>
        /// <param name="locationId">location id to use</param>
        /// <returns>found location, or null when no location could be found</returns>
        private Location FindLocationById(string locationId)
        {
            if (this.locationList == null)
            {
                return null;
            }

            return this.locationList.Find(location => location.Id == locationId);
        }

        /// <summary>
        /// Called when the user clicked on the "Share position" link in the "my position" pin description.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnMapView_ShareMyLocation()
        {
            var position =
                await this.geolocationService.GetPositionAsync(timeout: TimeSpan.FromSeconds(0.1));

            if (position != null)
            {
                var point = new MapPoint(position.Latitude, position.Longitude, position.Altitude);
                await App.UpdateLastShownPositionAsync(point);

                await App.ShareMessageAsync(
                    "Share my position with...",
                    DataFormatter.FormatMyPositionShareText(point, position.Timestamp));
            }
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
            var location = new Location
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = name ?? "Unknown",
                MapLocation = point,
                Description = string.Empty,
                Type = LocationType.Waypoint,
                InternetLink = string.Empty,
            };

            this.locationList.Add(location);

            var dataService = DependencyService.Get<IDataService>();
            var locationDataService = dataService.GetLocationDataService();

            await locationDataService.Add(location);

            await NavigationService.Instance.NavigateAsync(
                Constants.PageKeyEditLocationDetailsPage,
                animated: true,
                parameter: location);

            this.OnMessageUpdateMapLocations();
        }

        /// <summary>
        /// Called when the last shown location should be updated in the app settings.
        /// </summary>
        /// <param name="point">map point to store</param>
        /// <param name="viewingDistance">current viewing distance</param>
        /// <returns>task to wait on</returns>
        private async Task OnMapView_UpdateLastShownLocation(MapPoint point, int viewingDistance)
        {
            await App.UpdateLastShownPositionAsync(point, viewingDistance);
        }

        /// <summary>
        /// Called when the user performed a long-tap on the map
        /// </summary>
        /// <param name="point">map point where long-tap occured</param>
        /// <returns>task to wait on</returns>
        private async Task OnMapView_LongTap(MapPoint point)
        {
            string latitudeText = DataFormatter.FormatLatLong(point.Latitude, this.appSettings.CoordinateDisplayFormat);
            string longitudeText = DataFormatter.FormatLatLong(point.Longitude, this.appSettings.CoordinateDisplayFormat);

            var longTapActions = new List<string> { "Add new waypoint", "Navigate here", "Show flying range" };

            string result = await App.Current.MainPage.DisplayActionSheet(
                $"Selected point at Latitude: {latitudeText}, Longitude: {longitudeText}, Altitude {point.Altitude.GetValueOrDefault(0.0)} m",
                "Cancel",
                null,
                longTapActions.ToArray());

            if (!string.IsNullOrEmpty(result))
            {
                int selectedIndex = longTapActions.IndexOf(result);

                switch (selectedIndex)
                {
                    case 0:
                        await this.AddNewWaypoint(point);
                        break;

                    case 1:
                        await NavigateToPointAsync(string.Empty, point);
                        break;

                    case 2:
                        await this.ShowFlyingRange(point);
                        break;

                    default:
                        // ignore
                        break;
                }
            }
        }

        /// <summary>
        /// Adds new waypoint with given map point
        /// </summary>
        /// <param name="point">map point</param>
        /// <returns>task to wait on</returns>
        private async Task AddNewWaypoint(MapPoint point)
        {
            var location = new Location
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = string.Empty,
                MapLocation = point,
                Description = string.Empty,
                Type = LocationType.Waypoint,
                InternetLink = string.Empty,
            };

            this.locationList.Add(location);

            var dataService = DependencyService.Get<IDataService>();
            var locationDataService = dataService.GetLocationDataService();

            await locationDataService.Add(location);

            await NavigationService.Instance.NavigateAsync(
                Constants.PageKeyEditLocationDetailsPage,
                animated: true,
                parameter: location);

            this.OnMessageUpdateMapLocations();
        }

        /// <summary>
        /// Shows flying range from given point; shows a half transparent cone on the map that is
        /// created with some flying parameters like gliding ratio, wind spee and direction.
        /// </summary>
        /// <param name="point">point to calculate flying range for</param>
        /// <returns>task to wait on</returns>
        private async Task ShowFlyingRange(MapPoint point)
        {
            FlyingRangeParameters parameters = await FlyingRangePopupPage.ShowAsync();

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
            Location location = this.FindLocationById(locationId);

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
        private async Task AddTourPlanningLocationAsync(Location location)
        {
            this.planTourParameters.WaypointIdList.Add(location.Id);

            await PlanTourPopupPage.ShowAsync(this.planTourParameters);
        }

        /// <summary>
        /// Called when message arrives in order to update map settings
        /// </summary>
        private void OnMessageUpdateMapSettings()
        {
            this.ReloadMapViewAppSettings();
        }

        /// <summary>
        /// Called when message arrives in order to update location list on map
        /// </summary>
        private void OnMessageUpdateMapLocations()
        {
            App.RunOnUiThread(async () => await this.ReloadLocationListAsync());
        }

        /// <summary>
        /// Called when message arrives in order to update track list on map
        /// </summary>
        private void OnMessageUpdateMapTracks()
        {
            App.RunOnUiThread(async () => await this.ReloadTrackListAsync());
        }

        #region Page lifecycle methods
        /// <summary>
        /// Called when page is appearing; start position updates
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            Task.Run(async () =>
            {
                await this.geolocationService.StartListeningAsync();
            });

            this.geolocationService.PositionChanged += this.OnPositionChanged;

            Xamarin.Essentials.Connectivity.ConnectivityChanged += this.OnConnectivityChanged;
        }

        /// <summary>
        /// Called when network connectivity of the device has changed. Sends a notification to
        /// the map view.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnConnectivityChanged(object sender, Xamarin.Essentials.ConnectivityChangedEventArgs args)
        {
            if (this.mapView.MapInitializedTask.IsCompleted)
            {
                bool isConnectivityAvailable =
                    args.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet;

                this.mapView.OnNetworkConnectivityChanged(isConnectivityAvailable);
            }
        }

        /// <summary>
        /// Reloads map view app settings
        /// </summary>
        private void ReloadMapViewAppSettings()
        {
            this.appSettings = App.Settings;

            this.mapView.MapImageryType = this.appSettings.MapImageryType;
            this.mapView.MapOverlayType = this.appSettings.MapOverlayType;
            this.mapView.MapShadingMode = this.appSettings.ShadingMode;
            this.mapView.CoordinateDisplayFormat = this.appSettings.CoordinateDisplayFormat;
            this.mapView.UseEntityClustering = this.appSettings.UseMapEntityClustering;

            App.ShowToast("Settings were saved.");
        }

        /// <summary>
        /// Reloads location list from data service
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ReloadLocationListAsync()
        {
            var dataService = DependencyService.Get<IDataService>();
            var locationDataService = dataService.GetLocationDataService();

            var newLocationList = (await locationDataService.GetList()).ToList();

            this.AddAndRemoveDisplayedLocations(newLocationList);

            this.locationList = newLocationList;
        }

        /// <summary>
        /// Adds new and removes old locations from current location list
        /// </summary>
        /// <param name="newLocationList">list of new locations</param>
        private void AddAndRemoveDisplayedLocations(List<Location> newLocationList)
        {
            if (this.locationList == null ||
                !this.locationList.Any() ||
                Math.Abs(newLocationList.Count - this.locationList.Count) > 10)
            {
                this.ReplaceLocationList(newLocationList);
                return;
            }

            var locationIdsToRemove = new HashSet<string>();
            var locationsToUpdate = new HashSet<Location>();
            foreach (string oldLocationId in this.displayedLocationIds)
            {
                Location foundLocation = newLocationList.Find(locationToCheck => locationToCheck.Id == oldLocationId);
                if (foundLocation == null)
                {
                    locationIdsToRemove.Add(oldLocationId);
                }
                else
                {
                    var oldLocation = this.locationList.Find(locationIdToCheck => locationIdToCheck.Id == oldLocationId);
                    if (!foundLocation.Equals(oldLocation))
                    {
                        // when type has changed, re-add the location to generate a new entity
                        if (foundLocation.Type != oldLocation.Type)
                        {
                            locationIdsToRemove.Add(oldLocationId);
                        }
                        else
                        {
                            locationsToUpdate.Add(foundLocation);
                        }
                    }
                }
            }

            foreach (string locationIdToRemove in locationIdsToRemove)
            {
                this.mapView.RemoveLocation(locationIdToRemove);
                this.displayedLocationIds.Remove(locationIdToRemove);
            }

            foreach (var newLocation in newLocationList)
            {
                if (!this.displayedLocationIds.Contains(newLocation.Id))
                {
                    this.mapView.AddLocation(newLocation);
                    this.displayedLocationIds.Add(newLocation.Id);
                }
            }

            foreach (var modifiedLocation in locationsToUpdate)
            {
                this.mapView.UpdateLocation(modifiedLocation);
            }

            this.mapView.UpdateScene();
        }

        /// <summary>
        /// Replaces location list in map view
        /// </summary>
        /// <param name="newLocationList">new location list to use</param>
        private void ReplaceLocationList(List<Location> newLocationList)
        {
            if (this.locationList.Any())
            {
                this.mapView.ClearLocationList();
            }

            this.mapView.AddLocationList(newLocationList);

            this.displayedLocationIds = new HashSet<string>(
                this.locationList.Select(location => location.Id));
        }

        /// <summary>
        /// Reloads track list from data service
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ReloadTrackListAsync()
        {
            var dataService = DependencyService.Get<IDataService>();
            var trackDataService = dataService.GetTrackDataService();

            var newTrackList = (await trackDataService.GetList()).ToList();

            this.AddAndRemoveDisplayedTracks(newTrackList);

            this.trackList = newTrackList;
        }

        /// <summary>
        /// Adds new and removes old tracks from map view
        /// </summary>
        /// <param name="newTrackList">new track list</param>
        private void AddAndRemoveDisplayedTracks(List<Track> newTrackList)
        {
            var tracksToRemove = new HashSet<Track>();
            foreach (var oldTrack in this.displayedTracks)
            {
                if (!newTrackList.Contains(oldTrack))
                {
                    tracksToRemove.Add(oldTrack);
                }
            }

            foreach (var trackToRemove in tracksToRemove)
            {
                this.mapView.RemoveTrack(trackToRemove);
                this.displayedTracks.Remove(trackToRemove);
            }

            foreach (var newTrack in newTrackList)
            {
                if (!this.displayedTracks.Contains(newTrack))
                {
                    this.AddTrack(newTrack);
                }
            }
        }

        /// <summary>
        /// Called when form is disappearing; stop position updates
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            this.geolocationService.PositionChanged -= this.OnPositionChanged;
            Xamarin.Essentials.Connectivity.ConnectivityChanged -= this.OnConnectivityChanged;

            Task.Run(async () =>
            {
                await this.geolocationService.StopListeningAsync();
            });
        }
        #endregion

        /// <summary>
        /// Called when position has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args, including position</param>
        private void OnPositionChanged(object sender, GeolocationEventArgs args)
        {
            MapPoint point = args.Point;

            bool zoomToPosition = this.zoomToMyPosition;

            this.zoomToMyPosition = false;

            if (this.mapView != null)
            {
                this.mapView.UpdateMyLocation(
                    point,
                    (int)args.Position.Accuracy,
                    args.Position.Speed * Geo.Spatial.Constants.FactorMeterPerSecondToKilometerPerHour,
                    args.Position.Timestamp,
                    zoomToPosition);
            }

            if (zoomToPosition)
            {
                Task.Run(async () => await App.UpdateLastShownPositionAsync(point));
            }
        }
    }
}
