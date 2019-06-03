using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.Views;
using WhereToFly.App.Geo;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

// make Core internals visible to unit tests
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WhereToFly.App.UnitTest")]

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Xamarin.Forms application for the WhereToFly app
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Filename for the weather image cache file
        /// </summary>
        private const string WeatherImageCacheFilename = "weatherImageCache.json";

        /// <summary>
        /// Application settings
        /// </summary>
        public static AppSettings Settings { get; internal set; }

        /// <summary>
        /// The one and only map page (displaying the map using CesiumJS)
        /// </summary>
        public MapPage MapPage { get; internal set; }

        /// <summary>
        /// Creates a new app object
        /// </summary>
        public App()
        {
            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android ||
                Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.UWP)
            {
                AppCenter.Start(
                    $"android={Constants.AppCenterKeyAndroid};" +
                    $"uwp={Constants.AppCenterKeyUwp}",
                    typeof(Distribute),
                    typeof(Crashes));
            }

            this.InitializeComponent();
            this.SetupDepencencyService();
            this.SetupMainPage();
            Task.Run(async () => await this.LoadAppDataAsync());
        }

        /// <summary>
        /// Sets up DependencyService
        /// </summary>
        private void SetupDepencencyService()
        {
            DependencyService.Register<NavigationService>();
            DependencyService.Register<IDataService, DataService>();
            DependencyService.Register<GeolocationService>();
            DependencyService.Register<LiveWaypointRefreshService>();
        }

        /// <summary>
        /// Sets up MainPage; it contains a MapPage instance.
        /// </summary>
        private void SetupMainPage()
        {
            this.MapPage = new MapPage();

            var rootPage = new RootPage();
            this.MainPage = rootPage;
        }

        /// <summary>
        /// Loads app data needed for running, e.g. app settings, caches and location list for
        /// live waypoint refresh services.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task LoadAppDataAsync()
        {
            var dataService = DependencyService.Get<IDataService>();

            App.Settings = await dataService.GetAppSettingsAsync(CancellationToken.None);

            LoadWeatherImageCache();
            LoadFaviconUrlCache();
            await InitLiveWaypointRefreshService();
        }

        /// <summary>
        /// Logs exception that is occured and was caught, but is not presented to the user. Send
        /// it to AppCenter for further analysis.
        /// </summary>
        /// <param name="ex">exception to log</param>
        public static void LogError(Exception ex)
        {
            Crashes.TrackError(ex);
        }

        /// <summary>
        /// Runs action on the UI thread
        /// </summary>
        /// <param name="action">action to run</param>
        public static void RunOnUiThread(Action action)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(action);
        }

        /// <summary>
        /// Runs action on the UI thread and waits for completion; async version
        /// </summary>
        /// <param name="action">action to run</param>
        /// <returns>task to wait on for completion</returns>
        public static Task RunOnUiThreadAsync(Action action)
        {
            var tcs = new TaskCompletionSource<object>();

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    App.LogError(ex);
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Shows toast message with given text
        /// </summary>
        /// <param name="message">toast message text</param>
        public static void ShowToast(string message)
        {
            var app = Xamarin.Forms.Application.Current as App;

            MessagingCenter.Send<App, string>(app, Constants.MessageShowToast, message);
        }

        /// <summary>
        /// Shares a message with other apps
        /// </summary>
        /// <param name="title">title of the share dialog</param>
        /// <param name="message">message text to share</param>
        /// <returns>task to wait on</returns>
        public static async Task ShareMessageAsync(string title, string message)
        {
            await Xamarin.Essentials.Share.RequestAsync(message, title);
        }

        /// <summary>
        /// Clears cache of WebView used for map view and reloads map
        /// </summary>
        public static void ClearWebViewCache()
        {
            var app = Xamarin.Forms.Application.Current as App;

            MessagingCenter.Send(app, Constants.MessageWebViewClearCache);

            Task.Run(() => app.MapPage.ReloadMapAsync());
        }

        /// <summary>
        /// Adds new map layer
        /// </summary>
        /// <param name="layer">layer to add</param>
        internal static void AddMapLayer(Layer layer)
        {
            var app = Current as App;

            MessagingCenter.Send<App, Layer>(app, Constants.MessageAddLayer, layer);
        }

        /// <summary>
        /// Adds a track to the map view; when the map page is currently invisible, the add is
        /// carried out when the page appears.
        /// </summary>
        /// <param name="track">track to add</param>
        public static void AddMapTrack(Track track)
        {
            var app = Xamarin.Forms.Application.Current as App;

            MessagingCenter.Send<App, Track>(app, Constants.MessageAddTrack, track);
        }

        /// <summary>
        /// Adds a tour planning location to the current list of locations and opens the planning
        /// dialog.
        /// </summary>
        /// <param name="location">location to add</param>
        public static void AddTourPlanLocation(Location location)
        {
            var app = Current as App;

            MessagingCenter.Send<App, Location>(app, Constants.MessageAddTourPlanLocation, location);
        }

        /// <summary>
        /// Zooms to location on opened MapPage; when the map page is currently invisible, the
        /// zoom is carried out when the page appears.
        /// </summary>
        /// <param name="location">location to zoom to</param>
        public static void ZoomToLocation(MapPoint location)
        {
            var app = Xamarin.Forms.Application.Current as App;

            MessagingCenter.Send<App, MapPoint>(app, Constants.MessageZoomToLocation, location);
        }

        /// <summary>
        /// Zooms to track on opened MapPage; when the map page is currently invisible, the zoom
        /// is carried out when the page appears.
        /// </summary>
        /// <param name="track">track to zoom to</param>
        public static void ZoomToTrack(Track track)
        {
            var app = Xamarin.Forms.Application.Current as App;

            MessagingCenter.Send<App, Track>(app, Constants.MessageZoomToTrack, track);
        }

        /// <summary>
        /// Updates map settings on opened MapPage.
        /// </summary>
        public static void UpdateMapSettings()
        {
            var app = Xamarin.Forms.Application.Current as App;

            MessagingCenter.Send<App>(app, Constants.MessageUpdateMapSettings);
        }

        /// <summary>
        /// Updates locations list on opened MapPage.
        /// </summary>
        public static void UpdateMapLocationsList()
        {
            var app = Xamarin.Forms.Application.Current as App;

            MessagingCenter.Send<App>(app, Constants.MessageUpdateMapLocations);
        }

        /// <summary>
        /// Updates tracks list on opened MapPage.
        /// </summary>
        public static void UpdateMapTracksList()
        {
            var app = Xamarin.Forms.Application.Current as App;

            MessagingCenter.Send<App>(app, Constants.MessageUpdateMapTracks);
        }

        /// <summary>
        /// Opens file for importing data
        /// </summary>
        /// <param name="stream">stream to import</param>
        /// <param name="filename">filename of stream</param>
        /// <returns>task to wait on</returns>
        public async Task OpenFileAsync(Stream stream, string filename)
        {
            await OpenFileHelper.OpenFileAsync(stream, filename);
        }

        /// <summary>
        /// Opens app resource URI, e.g. a live waypoint
        /// </summary>
        /// <param name="uri">app resource URI to open</param>
        /// <returns>task to wait on</returns>
        public async Task OpenAppResourceUriAsync(string uri)
        {
            await OpenAppResourceUriHelper.Open(uri);
        }

        /// <summary>
        /// Loads the contents of the weather image cache
        /// </summary>
        private static void LoadWeatherImageCache()
        {
            var platform = DependencyService.Get<IPlatform>();
            string cacheFilename = Path.Combine(platform.CacheDataFolder, WeatherImageCacheFilename);

            if (File.Exists(cacheFilename))
            {
                try
                {
                    var imageCache = DependencyService.Get<WeatherImageCache>();
                    imageCache.LoadCache(cacheFilename);
                }
                catch (Exception ex)
                {
                    App.LogError(ex);
                }
            }
        }

        /// <summary>
        /// Loads the favicon url cache
        /// </summary>
        private static void LoadFaviconUrlCache()
        {
            try
            {
                var dataService = DependencyService.Get<IDataService>();
                (dataService as DataService).LoadFaviconUrlCache();
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }
        }

        /// <summary>
        /// Initializes live waypoint refresh service with current location list
        /// </summary>
        /// <returns>task to wait on</returns>
        private static async Task InitLiveWaypointRefreshService()
        {
            var dataService = DependencyService.Get<IDataService>();

            var locationList = await dataService.GetLocationListAsync(CancellationToken.None);

            var liveWaypointRefreshService = DependencyService.Get<LiveWaypointRefreshService>();
            liveWaypointRefreshService.DataService = dataService;

            liveWaypointRefreshService.UpdateLiveWaypointList(locationList);
        }

        /// <summary>
        /// Samples track heights for given track, updating track point altitudes in-place
        /// </summary>
        /// <param name="track">track to sample heights</param>
        /// <param name="offsetInMeters">offset in meters to add to track</param>
        /// <returns>task to wait on</returns>
        public static async Task SampleTrackHeightsAsync(Track track, double offsetInMeters)
        {
            var app = Current as App;

            await app.MapPage.SampleTrackHeightsAsync(track, offsetInMeters);
        }

        /// <summary>
        /// Stores the contents of the weather image cache in the cache folder
        /// </summary>
        private static void StoreWeatherImageCache()
        {
            var platform = DependencyService.Get<IPlatform>();
            string cacheFilename = Path.Combine(platform.CacheDataFolder, WeatherImageCacheFilename);

            var imageCache = DependencyService.Get<WeatherImageCache>();
            imageCache.StoreCache(cacheFilename);
        }

        /// <summary>
        /// Stores the contents of the favicon url cache in the cache folder
        /// </summary>
        private static void StoreFaviconUrlCache()
        {
            try
            {
                var dataService = DependencyService.Get<IDataService>();
                (dataService as DataService).StoreFaviconUrlCache();
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }
        }

        #region App lifecycle methods
        /// <summary>
        /// Called when application is starting
        /// </summary>
        protected override void OnStart()
        {
            // Handle when your app starts
        }

        /// <summary>
        /// Called when application is paused
        /// </summary>
        protected override void OnSleep()
        {
            // Handle when your app sleeps
            StoreWeatherImageCache();
            StoreFaviconUrlCache();
        }

        /// <summary>
        /// Called when application is resumed
        /// </summary>
        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        #endregion
    }
}
