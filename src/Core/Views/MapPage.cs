using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Share;
using Plugin.Share.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.Core.Services;
using WhereToFly.Logic;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core.Views
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
        /// Web view that displays the map
        /// </summary>
        private WebView webView;

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
        /// Creates a new maps page
        /// </summary>
        public MapPage()
        {
            this.zoomToMyPosition = false;
            this.startedSettingsPage = false;
            this.startedLocationListPage = false;

            this.geolocator = Plugin.Geolocator.CrossGeolocator.Current;

            Task.Factory.StartNew(this.InitLayoutAsync);
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
            this.AddLocationDetailsToolbarButton();
            this.AddSettingsToolbarButton();
            this.AddImportLocationsToolbarButton();
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
                Debug.WriteLine(ex.ToString());

                // zoom at next update
                this.zoomToMyPosition = true;

                return;
            }

            if (position != null &&
                Math.Abs(position.Latitude) < 1e5 &&
                Math.Abs(position.Longitude) < 1e5 &&
                this.mapView != null)
            {
                this.mapView.ZoomToLocation(
                    new MapPoint(position.Latitude, position.Longitude));
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
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Adds "Location details" toolbar button
        /// </summary>
        private void AddLocationDetailsToolbarButton()
        {
            ToolbarItem locationDetailsButton = new ToolbarItem(
                "Location Details",
                "compass.xml",
                async () => await this.OnClicked_ToolbarButtonLocationDetails(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "LocationDetails"
            };

            this.ToolbarItems.Add(locationDetailsButton);
        }

        /// <summary>
        /// Called when toolbar button "Location details" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonLocationDetails()
        {
            await NavigationService.Instance.NavigateAsync(Constants.PageKeyLocationDetailsPage, animated: true);
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
        /// Adds "Import locations" toolbar button
        /// </summary>
        private void AddImportLocationsToolbarButton()
        {
            ToolbarItem importLocationsButton = new ToolbarItem(
                "Import locations",
                "playlist_plus.xml",
                async () => await this.OnClicked_ToolbarButtonImportLocations(),
                ToolbarItemOrder.Secondary)
            {
                AutomationId = "ImportLocations"
            };

            this.ToolbarItems.Add(importLocationsButton);
        }

        /// <summary>
        /// Called when toolbar button "Import locations" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonImportLocations()
        {
            this.startedLocationListPage = true;
            await NavigationService.Instance.NavigateAsync(Constants.PageKeyImportLocationsPage, animated: true);
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

            this.webView = new WebView
            {
                Source = htmlSource,

                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            this.webView.AutomationId = "ExploreMapWebView";

            this.webView.Navigating += this.OnNavigating_WebView;
            this.webView.Navigated += this.OnNavigated_WebView;

            this.mapView = new MapView(this.webView);

            this.mapView.NavigateToLocation += this.OnMapView_NavigateToLocation;
            this.mapView.ShareMyLocation += async () => await this.OnMapView_ShareMyLocation();

            this.Content = this.webView;

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

            this.mapView.MapOverlayType = this.appSettings.MapOverlayType;
            this.mapView.MapShadingMode = this.appSettings.ShadingMode;

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

                this.mapView.MapOverlayType = this.appSettings.MapOverlayType;
                this.mapView.MapShadingMode = this.appSettings.ShadingMode;

                App.ShowToast("Settings were saved.");
            }

            if (this.startedLocationListPage)
            {
                this.startedLocationListPage = false;

                var dataService = DependencyService.Get<DataService>();

                this.locationList = await dataService.GetLocationListAsync(CancellationToken.None);

                this.mapView.ClearLocationList();
                this.mapView.AddLocationList(this.locationList);
            }
        }

        /// <summary>
        /// Called when form is disappearing; stop position updates
        /// </summary>
        protected override void OnDisappearing()
        {
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
                    new MapPoint(position.Latitude, position.Longitude), zoomToPosition);
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
            App.Settings.LastKnownPosition = new MapPoint(position.Latitude, position.Longitude);

            var dataService = DependencyService.Get<DataService>();
            await dataService.StoreAppSettingsAsync(App.Settings);
        }
    }
}
