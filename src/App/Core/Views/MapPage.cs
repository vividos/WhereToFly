using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Geo;
using WhereToFly.App.Geo.Spatial;
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
        /// Geo locator to use for position updates
        /// </summary>
        private readonly IGeolocator geolocator;

        /// <summary>
        /// List of track IDs currently being displayed
        /// </summary>
        private readonly HashSet<Track> displayedTracks = new HashSet<Track>();

        /// <summary>
        /// Tour planning parameters for the map page
        /// </summary>
        private readonly PlanTourParameters planTourParameters = new PlanTourParameters();

        /// <summary>
        /// Indicates if the next position update should also zoom to my position
        /// </summary>
        private bool zoomToMyPosition;

        /// <summary>
        /// Indicates if settings were updated while this page was invisible; used to update map
        /// view when returning to this page.
        /// </summary>
        private bool updateMapSettings;

        /// <summary>
        /// Indicates if locations list was changed while this page was invisible, used to update
        /// location list when returning to this page.
        /// </summary>
        private bool updateLocationsList;

        /// <summary>
        /// Indicates if track list was changed while this page was invisible; used to update
        /// track list when returning to this page.
        /// </summary>
        private bool updateTrackList;

        /// <summary>
        /// Location to zoom to when this map page is appearing next time
        /// </summary>
        private MapPoint zoomToLocationOnAppearing;

        /// <summary>
        /// Track to zoom to when this map page is appearing next time
        /// </summary>
        private Track zoomToTrackOnAppearing;

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
        /// Task completion source to signal that the web page has been loaded
        /// </summary>
        private TaskCompletionSource<bool> taskCompletionSourcePageLoaded;

        /// <summary>
        /// Indicates if this page is currently visible (OnAppearing() but no OnDisappearing()
        /// called).
        /// </summary>
        private bool pageIsVisible;

        /// <summary>
        /// Creates a new maps page
        /// </summary>
        public MapPage()
        {
            this.Title = Constants.AppTitle;

            this.pageIsVisible = false;
            this.zoomToMyPosition = false;
            this.updateMapSettings = false;
            this.updateLocationsList = false;
            this.updateTrackList = false;

            this.geolocator = DependencyService.Get<GeolocationService>().Geolocator;

            Task.Run(this.InitLayoutAsync);

            MessagingCenter.Subscribe<App, Track>(this, Constants.MessageAddTrack, this.OnMessageAddTrack);

            MessagingCenter.Subscribe<App, Location>(
                this,
                Constants.MessageAddTourPlanLocation,
                async (app, location) => await this.OnMessageAddTourPlanLocation(location));

            MessagingCenter.Subscribe<App, MapPoint>(this, Constants.MessageZoomToLocation, async (app, location) => await this.OnMessageZoomToLocation(location));
            MessagingCenter.Subscribe<App, Track>(this, Constants.MessageZoomToTrack, async (app, track) => await this.OnMessageZoomToTrack(track));
            MessagingCenter.Subscribe<App>(this, Constants.MessageUpdateMapSettings, this.OnMessageUpdateMapSettings);
            MessagingCenter.Subscribe<App>(this, Constants.MessageUpdateMapLocations, this.OnMessageUpdateMapLocations);
            MessagingCenter.Subscribe<App>(this, Constants.MessageUpdateMapTracks, this.OnMessageUpdateMapTracks);
        }

        /// <summary>
        /// Samples track heights for given track, updating track point altitudes in-place
        /// </summary>
        /// <param name="track">track to sample heights</param>
        /// <param name="offsetInMeters">offset in meters to add to track</param>
        /// <returns>task to wait on</returns>
        public async Task SampleTrackHeightsAsync(Track track, double offsetInMeters)
        {
            await this.mapView.SampleTrackHeights(track, offsetInMeters);
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
                Core.Converter.ImagePathConverter.GetDeviceDependentImage("crosshairs_gps"),
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
            if (!await this.CheckPermissionAsync())
            {
                return;
            }

            Plugin.Geolocator.Abstractions.Position position = null;
            try
            {
                position = await this.geolocator.GetPositionAsync(timeout: TimeSpan.FromMilliseconds(100), includeHeading: false);
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

                await this.UpdateLastShownPositionAsync(point);

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
        /// Checks for permission to use geolocator. See
        /// https://github.com/jamesmontemagno/PermissionsPlugin
        /// </summary>
        /// <returns>true when everything is ok, false when permission wasn't given</returns>
        private async Task<bool> CheckPermissionAsync()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);

                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                    {
                        await this.DisplayAlert(
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

            IEnumerable<Position> foundPositionsList = null;

            try
            {
                string mapKey = Device.RuntimePlatform == Device.UWP ? Constants.BingMapsKey : null;
                foundPositionsList = await this.geolocator.GetPositionsForAddressAsync(text, mapKey);
            }
            catch (Exception ex)
            {
                // ignore exceptions and just log; assume that no location was found
                App.LogError(ex);
            }

            if (foundPositionsList == null ||
                !foundPositionsList.Any())
            {
                await this.DisplayAlert(
                    Constants.AppTitle,
                    "The location could not be found",
                    "OK");

                return;
            }

            var position = foundPositionsList.First();
            var point = new MapPoint(position.Latitude, position.Longitude, position.Altitude);

            this.mapView.ShowFindResult(text, point);
        }

        /// <summary>
        /// Sets up WebView control
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task SetupWebViewAsync()
        {
            this.taskCompletionSourcePageLoaded = new TaskCompletionSource<bool>();

            var platform = DependencyService.Get<IPlatform>();

            WebViewSource webViewSource = null;
            if (Device.RuntimePlatform == Device.Android)
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
                Device.OpenUri(new Uri(args.Url));
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
            this.locationList = await dataService.GetLocationListAsync(CancellationToken.None);
            this.trackList = await dataService.GetTrackListAsync(CancellationToken.None);
        }

        /// <summary>
        /// Creates the map view
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task CreateMapViewAsync()
        {
            MapPoint initialCenter = this.appSettings.LastShownPosition ?? Constants.InitialCenterPoint;
            if (!initialCenter.Valid)
            {
                initialCenter = Constants.InitialCenterPoint;
            }

            await this.mapView.CreateAsync(initialCenter, 14);

            this.mapView.MapImageryType = this.appSettings.MapImageryType;
            this.mapView.MapOverlayType = this.appSettings.MapOverlayType;
            this.mapView.MapShadingMode = this.appSettings.ShadingMode;
            this.mapView.CoordinateDisplayFormat = this.appSettings.CoordinateDisplayFormat;

            this.mapView.AddLocationList(this.locationList);

            foreach (var track in this.trackList)
            {
                this.mapView.AddTrack(track);
                this.displayedTracks.Add(track);
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
                Task.Run(async() => await dataService.StoreLocationListAsync(this.locationList));
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
                await this.geolocator.GetPositionAsync(timeout: TimeSpan.FromSeconds(0.1), includeHeading: false);

            if (position != null)
            {
                var point = new MapPoint(position.Latitude, position.Longitude, position.Altitude);

                await this.UpdateLastShownPositionAsync(point);

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
            await dataService.StoreLocationListAsync(this.locationList);

            await NavigationService.Instance.NavigateAsync(
                Constants.PageKeyEditLocationDetailsPage,
                animated: true,
                parameter: location);

            this.updateLocationsList = true;
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
            await dataService.StoreLocationListAsync(this.locationList);

            await NavigationService.Instance.NavigateAsync(
                Constants.PageKeyEditLocationDetailsPage,
                animated: true,
                parameter: location);

            this.updateLocationsList = true;
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
        /// Called when message arrives in order to add a new track to map
        /// </summary>
        /// <param name="app">app object</param>
        /// <param name="track">track to add</param>
        private void OnMessageAddTrack(App app, Track track)
        {
            if (this.pageIsVisible)
            {
                App.RunOnUiThread(() => this.mapView.AddTrack(track));
            }
            else
            {
                this.updateTrackList = true;
            }
        }

        /// <summary>
        /// Called when message arrives in order to add tour plan location
        /// </summary>
        /// <param name="location">location to add</param>
        /// <returns>task to wait on</returns>
        private async Task OnMessageAddTourPlanLocation(Location location)
        {
            await this.AddTourPlanningLocationAsync(location);
        }

        /// <summary>
        /// Called when message arrives in order to zoom to a location
        /// </summary>
        /// <param name="location">location to zoom to</param>
        /// <returns>task to wait on</returns>
        private async Task OnMessageZoomToLocation(MapPoint location)
        {
            await this.UpdateLastShownPositionAsync(location);

            if (this.pageIsVisible)
            {
                this.mapView.ZoomToLocation(location);
            }
            else
            {
                this.zoomToLocationOnAppearing = location;
            }
        }

        /// <summary>
        /// Called when message arrives in order to zoom to a track
        /// </summary>
        /// <param name="track">track to zoom to</param>
        /// <returns>task to wait on</returns>
        private async Task OnMessageZoomToTrack(Track track)
        {
            var point = track.CalculateCenterPoint();

            if (point.Valid)
            {
                await this.UpdateLastShownPositionAsync(point);
            }

            if (this.pageIsVisible)
            {
                this.mapView.ZoomToTrack(track);
            }
            else
            {
                this.zoomToTrackOnAppearing = track;
            }
        }

        /// <summary>
        /// Called when message arrives in order to update map settings
        /// </summary>
        /// <param name="app">app object</param>
        private void OnMessageUpdateMapSettings(App app)
        {
            if (this.pageIsVisible)
            {
                App.RunOnUiThread(() => this.ReloadMapViewAppSettings());
            }
            else
            {
                this.updateMapSettings = true;
            }
        }

        /// <summary>
        /// Called when message arrives in order to update location list on map
        /// </summary>
        /// <param name="app">app object</param>
        private void OnMessageUpdateMapLocations(App app)
        {
            if (this.pageIsVisible)
            {
                App.RunOnUiThread(async () => await this.ReloadLocationListAsync());
            }
            else
            {
                this.updateLocationsList = true;
            }
        }

        /// <summary>
        /// Called when message arrives in order to update track list on map
        /// </summary>
        /// <param name="app">app object</param>
        private void OnMessageUpdateMapTracks(App app)
        {
            if (this.pageIsVisible)
            {
                App.RunOnUiThread(async () => await this.ReloadTrackListAsync());
            }
            else
            {
                this.updateTrackList = true;
            }
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
                await this.geolocator.StartListeningAsync(
                    Constants.GeoLocationMinimumTimeForUpdate,
                    Constants.GeoLocationMinimumDistanceForUpdateInMeters,
                    includeHeading: false);
            });

            this.geolocator.PositionChanged += this.OnPositionChanged;

            App.RunOnUiThread(async () => await this.CheckReloadData());

            this.pageIsVisible = true;
        }

        /// <summary>
        /// Checks if a reload of data into the map view is necessary
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task CheckReloadData()
        {
            if (this.updateMapSettings)
            {
                this.updateMapSettings = false;

                this.ReloadMapViewAppSettings();
            }

            if (this.updateLocationsList)
            {
                await this.ReloadLocationListAsync();
            }

            if (this.updateTrackList)
            {
                await this.ReloadTrackListAsync();
            }

            if (this.zoomToLocationOnAppearing != null)
            {
                this.mapView.ZoomToLocation(this.zoomToLocationOnAppearing);

                this.zoomToLocationOnAppearing = null;
            }

            if (this.zoomToTrackOnAppearing != null)
            {
                this.mapView.ZoomToTrack(this.zoomToTrackOnAppearing);

                this.zoomToTrackOnAppearing = null;
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

            App.ShowToast("Settings were saved.");
        }

        /// <summary>
        /// Reloads location list from data service
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ReloadLocationListAsync()
        {
            var dataService = DependencyService.Get<IDataService>();

            var newLocationList = await dataService.GetLocationListAsync(CancellationToken.None);

            if (this.updateLocationsList ||
                this.locationList.Count != newLocationList.Count ||
                !Enumerable.SequenceEqual(this.locationList, newLocationList, new LocationEqualityComparer()))
            {
                this.locationList = newLocationList;

                this.mapView.ClearLocationList();
                this.mapView.AddLocationList(this.locationList);

                this.updateLocationsList = false;
            }
        }

        /// <summary>
        /// Reloads track list from data service
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ReloadTrackListAsync()
        {
            var dataService = DependencyService.Get<IDataService>();

            var newTrackList = await dataService.GetTrackListAsync(CancellationToken.None);

            if (this.updateTrackList ||
                this.trackList.Count != newTrackList.Count ||
                !Enumerable.SequenceEqual(this.trackList, newTrackList, new TrackEqualityComparer()))
            {
                this.AddAndRemoveDisplayedTracks(newTrackList);

                this.trackList = newTrackList;

                this.updateTrackList = false;
            }
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
                    this.mapView.AddTrack(newTrack);
                    this.displayedTracks.Add(newTrack);
                }
            }
        }

        /// <summary>
        /// Called when form is disappearing; stop position updates
        /// </summary>
        protected override void OnDisappearing()
        {
            this.pageIsVisible = false;

            base.OnDisappearing();

            this.geolocator.PositionChanged -= this.OnPositionChanged;

            Task.Run(async () =>
            {
                await this.geolocator.StopListeningAsync();
            });
        }
        #endregion

        /// <summary>
        /// Called when position has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args, including position</param>
        private void OnPositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs args)
        {
            var position = args.Position;

            bool zoomToPosition = this.zoomToMyPosition;

            this.zoomToMyPosition = false;

            var point = new MapPoint(position.Latitude, position.Longitude, position.Altitude);

            if (this.mapView != null)
            {
                this.mapView.UpdateMyLocation(
                    point,
                    (int)position.Accuracy,
                    position.Speed * Geo.Spatial.Constants.FactorMeterPerSecondToKilometerPerHour,
                    position.Timestamp,
                    zoomToPosition);
            }

            Task.Run(async () => await this.UpdateLastShownPositionAsync(point));
        }

        /// <summary>
        /// Updates last shown position in data service
        /// </summary>
        /// <param name="point">position to store</param>
        /// <returns>task to wait on</returns>
        private async Task UpdateLastShownPositionAsync(MapPoint point)
        {
            if (this.appSettings == null)
            {
                return; // appSettings not loaded yet
            }

            if (point.Valid)
            {
                this.appSettings.LastShownPosition = point;

                var dataService = DependencyService.Get<IDataService>();
                await dataService.StoreAppSettingsAsync(this.appSettings);
            }
        }
    }
}
