using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Share;
using Plugin.Share.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Logic;
using WhereToFly.App.Logic.Model;
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
        /// Indicates if the next position update should also zoom to my position
        /// </summary>
        private bool zoomToMyPosition;

        /// <summary>
        /// Indicates if settings page was started; used to update settings when returning to this
        /// page.
        /// </summary>
        private bool startedSettingsPage;

        /// <summary>
        /// Location to zoom to when this map page is appearing next time
        /// </summary>
        private MapPoint zoomToLocationOnAppearing;

        /// <summary>
        /// Indicates if location list page was started; used to update location list when
        /// returning to this page.
        /// </summary>
        private bool startedLocationListPage;

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
            this.pageIsVisible = false;
            this.zoomToMyPosition = false;
            this.startedSettingsPage = false;
            this.startedLocationListPage = false;

            this.geolocator = Plugin.Geolocator.CrossGeolocator.Current;

            Task.Factory.StartNew(this.InitLayoutAsync);

            MessagingCenter.Subscribe<App, MapPoint>(this, Constants.MessageZoomToLocation, this.OnMessageZoomToLocation);
        }

        /// <summary>
        /// Initializes layout by loading map html into web view
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task InitLayoutAsync()
        {
            this.Title = Constants.AppTitle;

            App.RunOnUiThread(() => this.SetupToolbar());

            await this.SetupWebViewAsync();

            await this.LoadDataAsync();

            this.CreateMapView();
        }

        /// <summary>
        /// Sets up toolbar for this page
        /// </summary>
        private void SetupToolbar()
        {
            this.AddLocateMeToolbarButton();
            this.AddFindLocationToolbarButton();
            this.AddCurrentPositionDetailsToolbarButton();
            this.AddLocationListToolbarButton();
            this.AddSettingsToolbarButton();
            this.AddInfoToolbarButton();
        }

        /// <summary>
        /// Adds a "locate me" button to the toolbar
        /// </summary>
        private void AddLocateMeToolbarButton()
        {
            ToolbarItem locateMeButton = new ToolbarItem(
                "Locate me",
                "crosshairs_gps.xml",
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

            Position position = null;
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
                await this.UpdateLastKnownPositionAsync(position);

                this.mapView.UpdateMyLocation(
                    new MapPoint(position.Latitude, position.Longitude),
                    (int)position.Altitude,
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
                "magnify.xml",
                async () => await this.OnClicked_ToolbarButtonFindLocation(),
                ToolbarItemOrder.Secondary)
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
                foundPositionsList = await this.geolocator.GetPositionsForAddressAsync(text);
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
            var point = new MapPoint(position.Latitude, position.Longitude);

            this.mapView.ShowFindResult(text, point);
        }

        /// <summary>
        /// Adds "Current position details" toolbar button
        /// </summary>
        private void AddCurrentPositionDetailsToolbarButton()
        {
            ToolbarItem currentPositionDetailsButton = new ToolbarItem(
                "Current Position",
                "compass.xml",
                async () => await this.OnClicked_ToolbarButtonCurrentPositionDetails(),
                ToolbarItemOrder.Secondary)
            {
                AutomationId = "CurrentPosition"
            };

            this.ToolbarItems.Add(currentPositionDetailsButton);
        }

        /// <summary>
        /// Called when toolbar button "Current position details" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonCurrentPositionDetails()
        {
            await NavigationService.Instance.NavigateAsync(Constants.PageKeyCurrentPositionDetailsPage, animated: true);
        }

        /// <summary>
        /// Adds "Settings" toolbar button
        /// </summary>
        private void AddSettingsToolbarButton()
        {
            ToolbarItem settingsButton = new ToolbarItem(
                "Settings",
                "settings.xml",
                async () => await this.OnClicked_ToolbarButtonSettings(),
                ToolbarItemOrder.Secondary)
            {
                AutomationId = "Settings"
            };

            this.ToolbarItems.Add(settingsButton);
        }

        /// <summary>
        /// Called when toolbar button "Settings" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonSettings()
        {
            this.startedSettingsPage = true;
            await NavigationService.Instance.NavigateAsync(Constants.PageKeySettingsPage, animated: true);
        }

        /// <summary>
        /// Adds "Location list" toolbar button
        /// </summary>
        private void AddLocationListToolbarButton()
        {
            ToolbarItem locationListButton = new ToolbarItem(
                "Location list",
                "format_list_bulleted.xml",
                async () => await this.OnClicked_ToolbarButtonLocationList(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "LocationList"
            };

            this.ToolbarItems.Add(locationListButton);
        }

        /// <summary>
        /// Called when toolbar button "Location list" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonLocationList()
        {
            this.startedLocationListPage = true;
            await NavigationService.Instance.NavigateAsync(Constants.PageKeyLocationListPage, animated: true);
        }

        /// <summary>
        /// Adds "Info" toolbar button
        /// </summary>
        private void AddInfoToolbarButton()
        {
            ToolbarItem infoButton = new ToolbarItem(
                "Info",
                "information_outline.xml",
                async () => await this.OnClicked_ToolbarButtonInfo(),
                ToolbarItemOrder.Secondary)
            {
                AutomationId = "Info"
            };

            this.ToolbarItems.Add(infoButton);
        }

        /// <summary>
        /// Called when toolbar button "Info" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonInfo()
        {
            await NavigationService.Instance.NavigateAsync(Constants.PageKeyInfoPage, animated: true);
        }

        /// <summary>
        /// Sets up WebView control
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task SetupWebViewAsync()
        {
            this.taskCompletionSourcePageLoaded = new TaskCompletionSource<bool>();

            var platform = DependencyService.Get<IPlatform>();

            string htmlText = platform.LoadAssetText("map/map3D.html");

            var htmlSource = new HtmlWebViewSource
            {
                Html = htmlText,
                BaseUrl = platform.WebViewBasePath + "map/"
            };

            var webView = new WebView
            {
                Source = htmlSource,

                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            webView.AutomationId = "ExploreMapWebView";

            webView.Navigating += this.OnNavigating_WebView;
            webView.Navigated += this.OnNavigated_WebView;

            this.mapView = new MapView(webView);

            this.mapView.NavigateToLocation += this.OnMapView_NavigateToLocation;
            this.mapView.ShareMyLocation += async () => await this.OnMapView_ShareMyLocation();
            this.mapView.AddFindResult += async (name, point) => await this.OnMapView_AddFindResult(name, point);

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
            this.taskCompletionSourcePageLoaded.SetResult(true);
        }

        /// <summary>
        /// Loads data; async method
        /// </summary>
        /// <returns>app info object</returns>
        private async Task LoadDataAsync()
        {
            var dataService = DependencyService.Get<DataService>();

            this.appSettings = await dataService.GetAppSettingsAsync(CancellationToken.None);
            this.locationList = await dataService.GetLocationListAsync(CancellationToken.None);
        }

        /// <summary>
        /// Creates the map view
        /// </summary>
        private void CreateMapView()
        {
            MapPoint initialCenter = this.appSettings.LastKnownPosition ?? new MapPoint(0.0, 0.0);

            this.mapView.Create(initialCenter, 14);

            this.mapView.MapImageryType = this.appSettings.MapImageryType;
            this.mapView.MapOverlayType = this.appSettings.MapOverlayType;
            this.mapView.MapShadingMode = this.appSettings.ShadingMode;
            this.mapView.CoordinateDisplayFormat = this.appSettings.CoordinateDisplayFormat;

            this.mapView.AddLocationList(this.locationList);
        }

        /// <summary>
        /// Called when user clicked on the "Navigate here" link in the pin description on the
        /// map.
        /// </summary>
        /// <param name="locationId">location id of location to navigate to</param>
        private void OnMapView_NavigateToLocation(string locationId)
        {
            Location location = this.FindLocationById(locationId);

            if (location == null)
            {
                Debug.WriteLine("couldn't find location with id=" + locationId);
                return;
            }

            Plugin.ExternalMaps.CrossExternalMaps.Current.NavigateTo(
                location.Name,
                location.MapLocation.Latitude,
                location.MapLocation.Longitude,
                Plugin.ExternalMaps.Abstractions.NavigationType.Driving);
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
                await this.UpdateLastKnownPositionAsync(position);

                var point = new MapPoint(position.Latitude, position.Longitude);

                await CrossShare.Current.Share(
                    new ShareMessage
                    {
                        Title = Constants.AppTitle,
                        Text = DataFormatter.FormatMyPositionShareText(point, position.Altitude, position.Timestamp)
                    },
                    new ShareOptions
                    {
                        ChooserTitle = "Share my position with..."
                    });
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
                Elevation = 0,
                MapLocation = point,
                Description = string.Empty,
                Type = LocationType.Waypoint,
                InternetLink = string.Empty,
            };

            this.locationList.Add(location);

            var dataService = DependencyService.Get<DataService>();
            await dataService.StoreLocationListAsync(locationList);

            await this.ReloadLocationListAsync();
        }

        /// <summary>
        /// Called when message arrives in order to zoom to a location
        /// </summary>
        /// <param name="app">app object</param>
        /// <param name="location">location to zoom to</param>
        private void OnMessageZoomToLocation(App app, MapPoint location)
        {
            if (this.pageIsVisible)
            {
                this.mapView.ZoomToLocation(location);
            }
            else
            {
                this.zoomToLocationOnAppearing = location;
            }
        }

        #region Page lifecycle methods
        /// <summary>
        /// Called when page is appearing; start position updates
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            Task.Factory.StartNew(async () =>
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
            if (this.startedSettingsPage)
            {
                this.startedSettingsPage = false;

                this.appSettings = App.Settings;

                this.mapView.MapImageryType = this.appSettings.MapImageryType;
                this.mapView.MapOverlayType = this.appSettings.MapOverlayType;
                this.mapView.MapShadingMode = this.appSettings.ShadingMode;
                this.mapView.CoordinateDisplayFormat = this.appSettings.CoordinateDisplayFormat;

                App.ShowToast("Settings were saved.");
            }

            if (this.startedLocationListPage)
            {
                this.startedLocationListPage = false;

                await this.ReloadLocationListAsync();
            }

            if (this.zoomToLocationOnAppearing != null)
            {
                this.mapView.ZoomToLocation(this.zoomToLocationOnAppearing);

                this.zoomToLocationOnAppearing = null;
            }
        }

        /// <summary>
        /// Reloads location list from data service
        /// </summary>
        /// <returns>task to wait on</returns>
        internal async Task ReloadLocationListAsync()
        {
            var dataService = DependencyService.Get<DataService>();

            var newLocationList = await dataService.GetLocationListAsync(CancellationToken.None);

            if (!Enumerable.SequenceEqual(this.locationList, newLocationList))
            {
                this.locationList = newLocationList;

                this.mapView.ClearLocationList();
                this.mapView.AddLocationList(this.locationList);
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

            Task.Factory.StartNew(async () =>
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

            if (this.mapView != null)
            {
                this.mapView.UpdateMyLocation(
                    new MapPoint(position.Latitude, position.Longitude),
                    (int)position.Altitude,
                    position.Timestamp,
                    zoomToPosition);
            }

            Task.Factory.StartNew(async () => await this.UpdateLastKnownPositionAsync(position));
        }

        /// <summary>
        /// Updates last known position in data service
        /// </summary>
        /// <param name="position">current position</param>
        /// <returns>task to wait on</returns>
        private async Task UpdateLastKnownPositionAsync(Position position)
        {
            var point = new MapPoint(position.Latitude, position.Longitude);

            if (point.Valid)
            {
                this.appSettings.LastKnownPosition = point;

                var dataService = DependencyService.Get<DataService>();
                await dataService.StoreAppSettingsAsync(this.appSettings);
            }
        }
    }
}
