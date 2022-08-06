using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.Styles;
using WhereToFly.App.Core.Views;
using WhereToFly.App.MapView;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

// make Core internals visible to unit tests
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WhereToFly.App.UnitTest")]

// compile all xaml pages
[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Xamarin.Forms application for the WhereToFly app
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Task completion source which task is completed when the app has finished
        /// initialisation
        /// </summary>
        private static readonly TaskCompletionSource<bool> TaskCompletionSourceInitialized
            = new();

        /// <summary>
        /// Application settings
        /// </summary>
        public static AppSettings Settings { get; internal set; }

        /// <summary>
        /// The one and only map page (displaying the map using CesiumJS)
        /// </summary>
        public MapPage MapPage { get; internal set; }

        /// <summary>
        /// Access to the map view instance
        /// </summary>
        public static IMapView MapView => (Current as App).MapPage.MapView;

        /// <summary>
        /// Task that can be awaited to wait for a completed app initialisation. The task performs
        /// the following:
        /// - sets up dependency service objects
        /// - sets up main page and map page
        /// - loads app data
        /// - initializes live waypoint refresh service
        /// Note that MapPage also has a task to wait for initialized page.
        /// </summary>
        public static Task InitializedTask => TaskCompletionSourceInitialized.Task;

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

            TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;

            this.InitializeComponent();
            this.SetupDepencencyService();
            this.SetupMainPage();

            if (!TaskCompletionSourceInitialized.Task.IsCompleted)
            {
                Task.Run(async () => await this.LoadAppDataAsync());
            }

            this.RequestedThemeChanged += this.OnRequestedThemeChanged;
        }

        /// <summary>
        /// Called when an exception was thrown in a Task and nobody caught it
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            Exception ex = args.Exception.InnerExceptions.Count == 1
                ? args.Exception.InnerException
                : args.Exception;

            Debug.WriteLine($"UnobservedTaskException: {ex}");

            LogError(ex);
        }

        /// <summary>
        /// Called when the requested theme of the operating system has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs args)
        {
            Debug.WriteLine($"OS App Theme changed to {args.RequestedTheme}");

            if (Settings != null)
            {
                ThemeHelper.ChangeTheme(Settings.AppTheme, true);
            }
        }

        /// <summary>
        /// Sets up DependencyService
        /// </summary>
        private void SetupDepencencyService()
        {
            DependencyService.Register<SvgImageCache>();
            DependencyService.Register<NavigationService>();
            DependencyService.Register<IDataService, Services.SqliteDatabase.SqliteDatabaseDataService>();
            DependencyService.Register<IGeolocationService, GeolocationService>();
            DependencyService.Register<LiveDataRefreshService>();
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
            Settings = await dataService.GetAppSettingsAsync(CancellationToken.None);

            ThemeHelper.ChangeTheme(Settings.AppTheme, true);

            await InitLiveWaypointRefreshService();

            TaskCompletionSourceInitialized.SetResult(true);
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
            var app = Current as App;

            MessagingCenter.Send(app, Constants.MessageShowToast, message);
        }

        /// <summary>
        /// Returns color hex string for given resource color key
        /// </summary>
        /// <param name="colorKey">resource color key</param>
        /// <returns>hex color string, in the format #RRGGBB</returns>
        public static string GetResourceColor(string colorKey)
        {
            return Current?.Resources != null &&
                Current.Resources.TryGetValue(colorKey, out object value) &&
                value is Color color
                ? color.ToHex().Replace("#FF", "#")
                : "#000000";
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
        /// Adds a tour planning location to the current list of locations and opens the planning
        /// dialog.
        /// </summary>
        /// <param name="location">location to add</param>
        public static void AddTourPlanLocation(Location location)
        {
            var app = Current as App;

            MessagingCenter.Send(app, Constants.MessageAddTourPlanLocation, location);
        }

        /// <summary>
        /// Adds track to map view
        /// </summary>
        /// <param name="track">track to add</param>
        /// <returns>task to wait on</returns>
        public static async Task AddTrack(Track track)
        {
            var point = track.CalculateCenterPoint();
            await UpdateLastShownPositionAsync(point);

            MapView.AddTrack(track);
        }

        /// <summary>
        /// Updates map settings on opened MapPage.
        /// </summary>
        public static void UpdateMapSettings()
        {
            var app = Current as App;

            MessagingCenter.Send(app, Constants.MessageUpdateMapSettings);
        }

        /// <summary>
        /// Updates last shown position in app settings
        /// </summary>
        /// <param name="point">current position</param>
        /// <param name="viewingDistance">current viewing distance; may be unset</param>
        /// <returns>task to wait on</returns>
        public static async Task UpdateLastShownPositionAsync(MapPoint point, int? viewingDistance = null)
        {
            if (Settings == null)
            {
                return; // app settings not loaded yet
            }

            if (point.Valid)
            {
                Settings.LastShownPosition = point;

                if (viewingDistance.HasValue)
                {
                    Settings.LastViewingDistance = viewingDistance.Value;
                }

                var dataService = DependencyService.Get<IDataService>();
                await dataService.StoreAppSettingsAsync(Settings);
            }
        }

        /// <summary>
        /// Sets (or clears) the current compass target
        /// </summary>
        /// <param name="compassTarget">compass target; may be null</param>
        /// <returns>task to wait on</returns>
        public static async Task SetCompassTarget(CompassTarget compassTarget)
        {
            if (Settings == null)
            {
                return; // app settings not loaded yet
            }

            Settings.CurrentCompassTarget = compassTarget;

            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreAppSettingsAsync(Settings);

            if (compassTarget == null)
            {
                MapView.ClearCompass();
            }
            else
            {
                if (compassTarget.TargetLocation != null)
                {
                    MapView.SetCompassTarget(
                        compassTarget.Title,
                        compassTarget.TargetLocation,
                        zoomToPolyline: true);
                }
                else
                {
                    Debug.Assert(
                        compassTarget.TargetDirection.HasValue,
                        "either target location or target direction must be set");

                    MapView.SetCompassDirection(
                        compassTarget.Title,
                        compassTarget.TargetDirection ?? 0);
                }
            }
        }

        /// <summary>
        /// Opens app resource URI, e.g. a live waypoint
        /// </summary>
        /// <param name="uri">app resource URI to open</param>
        public static void OpenAppResourceUri(string uri)
        {
            RunOnUiThread(async () => await OpenAppResourceUriHelper.OpenAsync(uri));
        }

        /// <summary>
        /// Initializes live waypoint refresh service with current location list
        /// </summary>
        /// <returns>task to wait on</returns>
        private static async Task InitLiveWaypointRefreshService()
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
        public static async Task ShowFlightPlanningDisclaimerAsync()
        {
            var dataService = DependencyService.Get<IDataService>();
            var appSettings = await dataService.GetAppSettingsAsync(CancellationToken.None);

            if (appSettings.ShownFlightPlanningDisclaimer)
            {
                return;
            }

            await Xamarin.Forms.Device.InvokeOnMainThreadAsync(async () =>
            {
                const string DisclaimerMessage =
                    "The display and use of flight maps and airspace data can contain errors " +
                    "and their use does not release the pilot from the legal obligation of " +
                    "thorough and orderly preflight planning, nor from the use of all required " +
                    "and approved means of navigation (e.g. Aeronautical Chart ICAO 1:500,000).";

                await Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    DisclaimerMessage,
                    "Understood");
            });

            appSettings.ShownFlightPlanningDisclaimer = true;
            await dataService.StoreAppSettingsAsync(appSettings);
        }

        #region App lifecycle methods
        /// <summary>
        /// Called when application is starting
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            if (Settings != null)
            {
                ThemeHelper.ChangeTheme(Settings.AppTheme, true);
            }
        }

        /// <summary>
        /// Called when application is paused
        /// </summary>
        protected override void OnSleep()
        {
            base.OnSleep();

            var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveWaypointRefreshService.StopTimer();

            Task.Run(async () =>
            {
                var geolocationService = DependencyService.Get<IGeolocationService>();
                await geolocationService.StopListeningAsync();
            });
        }

        /// <summary>
        /// Called when application is resumed
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();

            if (Settings != null)
            {
                ThemeHelper.ChangeTheme(Settings.AppTheme, true);
            }

            var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveWaypointRefreshService.ResumeTimer();

            Task.Run(async () =>
            {
                var geolocationService = DependencyService.Get<IGeolocationService>();
                await geolocationService.StartListeningAsync();
            });
        }
        #endregion
    }
}
