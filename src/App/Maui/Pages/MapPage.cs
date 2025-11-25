using System.Diagnostics;
using WhereToFly.App.Abstractions;
using WhereToFly.App.Behaviors;
using WhereToFly.App.MapView.Abstractions;
using WhereToFly.App.Services;
using WhereToFly.App.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Pages
{
    /// <summary>
    /// Page showing a map, with pins for locations loaded into the app.
    /// </summary>
    public class MapPage : ContentPage
    {
        /// <summary>
        /// Geolocation service to use for position updates
        /// </summary>
        private readonly IGeolocationService geolocationService;

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
        /// Access to the map view instance
        /// </summary>
        internal IMapView MapView => this.mapView
            ?? throw new InvalidOperationException("accessing MapView before it is initialized");

        /// <summary>
        /// Creates a new maps page
        /// </summary>
        /// <param name="services">services</param>
        public MapPage(IServiceProvider services)
        {
            var appMapService = services.GetRequiredService<IAppMapService>();
            var dataService = services.GetRequiredService<IDataService>();

            this.Title = Constants.AppTitle;
            this.BackgroundColor = Colors.Black;

            this.geolocationService = services.GetRequiredService<IGeolocationService>();

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
                this.mapView,
                appMapService,
                dataService,
                this.geolocationService);

            this.viewModel.PropertyChanged += this.OnViewModelPropertyChanged;

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
        /// Called when a binding property of the view model has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnViewModelPropertyChanged(
            object? sender,
            System.ComponentModel.PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(MapPageViewModel.IsVisiblePlanTourButton))
            {
                this.UpdatePlanTourButtonVisibility(
                    this.viewModel.IsVisiblePlanTourButton);
            }
        }

        /// <summary>
        /// Initializes layout by loading map html into web view
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task InitLayoutAsync()
        {
            this.SetupToolbar();

            this.SetupWebView();

            await this.viewModel.InitViewModel();
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
                async () => await this.viewModel.ShowPlanTourPopup(),
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
                async () => await this.viewModel.OnClickedFindNearbyPois(),
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
                async () => await this.viewModel.OnClickedLocateMe(),
                ToolbarItemOrder.Primary,
                2)
            {
                AutomationId = "LocateMe",
            };

            this.ToolbarItems.Add(locateMeButton);
        }

        /// <summary>
        /// Adds "Find location" toolbar button
        /// </summary>
        private void AddFindLocationToolbarButton()
        {
            var currentPositionDetailsButton = new ToolbarItem(
                "Find location",
                "magnify.png",
                async () => await this.viewModel.OnClickedFindLocation(),
                ToolbarItemOrder.Primary,
                3)
            {
                AutomationId = "FindLocation",
            };

            this.ToolbarItems.Add(currentPositionDetailsButton);
        }

        /// <summary>
        /// Sets up WebView control. Loads the map html page into the web view, creates the C#
        /// MapView instance.
        /// </summary>
        private void SetupWebView()
        {
            this.mapView.Behaviors.Add(
                new OpenLinkExternalBrowserWebViewBehavior());

            this.mapView.ShowLocationDetails += async (locationId) => await this.viewModel.OnShowLocationDetails(locationId);
            this.mapView.NavigateToLocation += async (locationId) => await this.viewModel.OnNavigateToLocation(locationId);
            this.mapView.ShareMyLocation += () => this.OnMapView_ShareMyLocation();
            this.mapView.AddFindResult += async (name, point) => await this.viewModel.OnAddFindResult(name, point);
            this.mapView.LongTap += async (point) => await this.viewModel.OnLongTap(point);
            this.mapView.AddTourPlanLocation += async (locationId) => await this.viewModel.OnAddTourPlanLocation(locationId);
            this.mapView.AddTempTourPlanPoint += async (point) => await this.viewModel.AddTempPlanTourPoint(point);
            this.mapView.UpdateLastShownLocation += async (point, viewingDistance)
                => await this.viewModel.OnUpdateLastShownLocation(point, viewingDistance);
            this.mapView.SetLocationAsCompassTarget += async (locationId)
                => await this.viewModel.OnSetLocationAsCompassTarget(locationId);
            this.mapView.ShowTrackDetails += async (locationId) => await this.viewModel.OnShowTrackDetails(locationId);

            // UWP needs to create the renderer in the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                this.Content = this.mapView;
            });
        }

        /// <summary>
        /// Called when the user clicked on the "Share position" link in the "my position" pin description.
        /// </summary>
        private void OnMapView_ShareMyLocation()
        {
            this.viewModel.ShareMyLocationCommand.Execute(null);
        }

        /// <summary>
        /// Adds new waypoint with given map point
        /// </summary>
        /// <param name="point">map point</param>
        /// <returns>task to wait on</returns>
        public async Task AddNewWaypoint(MapPoint point)
            => await this.viewModel.AddNewWaypoint(point);

        /// <summary>
        /// Called to add a location to tour planning
        /// </summary>
        /// <param name="location">location to add</param>
        /// <returns>task to wait on</returns>
        public async Task AddTourPlanningLocationAsync(Location location)
            => await this.viewModel.AddTourPlanningLocationAsync(location);

        /// <summary>
        /// Clears all temporary plan tour locations after planning tour
        /// </summary>
        /// <returns>task to wait on</returns>
        internal async Task ClearTempPlanTourLocations()
            => await this.viewModel.ClearTempPlanTourLocations();

        #region Page lifecycle methods
        /// <summary>
        /// Called when page is appearing; start position updates
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            this.geolocationService.PositionChanged += this.viewModel.OnPositionChanged;

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
            var appSettings = App.Settings;

            if (appSettings == null)
            {
                Debug.Assert(false, "calling ReloadMapViewAppSettings() before data was loaded!");
                return;
            }

            this.mapView.MapImageryType = appSettings.MapImageryType;
            this.mapView.MapOverlayType = appSettings.MapOverlayType;
            this.mapView.MapShadingMode = appSettings.ShadingMode;
            this.mapView.CoordinateDisplayFormat = appSettings.CoordinateDisplayFormat;
            this.mapView.UseEntityClustering = appSettings.UseMapEntityClustering;
        }

        /// <summary>
        /// Called when form is disappearing; stop position updates
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            this.geolocationService.PositionChanged -= this.viewModel.OnPositionChanged;

            Connectivity.ConnectivityChanged -= this.OnConnectivityChanged;
        }
        #endregion
    }
}
