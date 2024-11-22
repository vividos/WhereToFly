using FFImageLoading;
using FFImageLoading.Config;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using System.Diagnostics;
using WhereToFly.App.Logic;
using WhereToFly.App.Models;
using WhereToFly.App.Pages;
using WhereToFly.App.Services;
using WhereToFly.Shared.Model;

// make app internals visible to unit tests
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WhereToFly.App.UnitTest")]

// compile all xaml pages
[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace WhereToFly.App
{
    /// <summary>
    /// WhereToFly app
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Task completion source used for the InitializedTask property
        /// </summary>
        private static readonly TaskCompletionSource<bool> TaskCompletionSourceInitialized = new();

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
        /// Application configuration, e.g. API keys; this is retrieved from the backend and is
        /// available when <see cref="InitializedTask"/> has completed.
        /// </summary>
        public static AppConfig? Config { get; internal set; }

        /// <summary>
        /// Application settings; this is initialized when <see cref="InitializedTask"/> has
        /// completed, and is available in every content page.
        /// </summary>
        public static AppSettings? Settings { get; internal set; }

        /// <summary>
        /// Indicates if listening to location updates should be restarted when resuming the app
        /// </summary>
        private bool restartLocationUpdates;

        /// <summary>
        /// Creates a new app object
        /// </summary>
        /// <param name="services">service provider</param>
        public App(IServiceProvider services)
        {
            if (DeviceInfo.Platform == DevicePlatform.Android ||
                DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                AppCenter.Start(
                    $"android={Constants.AppCenterKeyAndroid};" +
                    $"windowsdesktop={Constants.AppCenterKeyWindows}",
                    typeof(Distribute),
                    typeof(Crashes));
            }

            var imageService = services.GetService<IImageService>();
            imageService?.Initialize(
                new Configuration
                {
                    HttpClient = new HttpClient(
                        new FFImageLoadingHttpClientHandler()),
                });

            TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;

            this.InitializeComponent();

            SetupDepencencyService();

            Task.Run(this.LoadAppDataAsync);

            this.RequestedThemeChanged += this.OnRequestedThemeChanged;
        }

        /// <summary>
        /// Called when the app's window is about to be created. Sets the app's title.
        /// </summary>
        /// <param name="activationState">activation state</param>
        /// <returns>window object</returns>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window
            {
                Title = Constants.AppTitle,
                Page = new RootPage(),
            };
        }

        /// <summary>
        /// Called when the requested theme of the operating system has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs args)
        {
            Debug.WriteLine($"OS App Theme changed to {args.RequestedTheme}");

            if (Settings != null)
            {
                var userInterface = DependencyService.Get<IUserInterface>();
                userInterface.UserAppTheme = Settings.AppTheme;
            }
        }

        /// <summary>
        /// Sets up DependencyService
        /// </summary>
        private static void SetupDepencencyService()
        {
            DependencyService.Register<IUserInterface, UserInterface>();
            DependencyService.Register<IAppMapService, AppMapService>();
            DependencyService.Register<SvgImageCache>();
            DependencyService.Register<INavigationService, NavigationService>();
            DependencyService.Register<IDataService, Services.SqliteDatabase.SqliteDatabaseDataService>();
            DependencyService.Register<IGeolocationService, GeolocationService>();
            DependencyService.Register<LiveDataRefreshService>();

#if ANDROID
            DependencyService.Register<IPlatform, AndroidPlatform>();
#elif WINDOWS
            DependencyService.Register<IPlatform, WindowsPlatform>();
#endif
        }

        /// <summary>
        /// Loads app data needed for running, e.g. app settings, caches and location list for
        /// live waypoint refresh services.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task LoadAppDataAsync()
        {
            var dataService = DependencyService.Get<IDataService>();
            Config = await dataService.GetAppConfigAsync(CancellationToken.None);
            Settings = await dataService.GetAppSettingsAsync(CancellationToken.None);

            var userInterface = DependencyService.Get<IUserInterface>();
            userInterface.UserAppTheme = Settings.AppTheme;

            var appMapService = DependencyService.Get<IAppMapService>();
            await appMapService.InitLiveWaypointRefreshService();

            TaskCompletionSourceInitialized.SetResult(true);
        }

        /// <summary>
        /// Called when an exception was thrown in a Task and nobody caught it
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void TaskScheduler_UnobservedTaskException(
            object? sender,
            UnobservedTaskExceptionEventArgs args)
        {
            Exception? ex = args.Exception.InnerExceptions.Count == 1
                ? args.Exception.InnerException
                : args.Exception;

            Debug.WriteLine($"UnobservedTaskException: {ex}");

            if (ex != null)
            {
                LogError(ex);
            }
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
        /// Returns color hex string for given resource color key
        /// </summary>
        /// <param name="colorKey">resource color key</param>
        /// <param name="addThemeSuffix">
        /// when true, adds the theme specific suffix to the resource color key, e.g. Dark or
        /// Light
        /// </param>
        /// <returns>hex color string, in the format #RRGGBB</returns>
        public static string GetResourceColor(string colorKey, bool addThemeSuffix)
        {
            if (addThemeSuffix)
            {
                bool isDarkTheme =
                    Current!.UserAppTheme == AppTheme.Dark ||
                    (Current!.UserAppTheme == AppTheme.Unspecified &&
                     Current!.RequestedTheme == AppTheme.Dark);

                string themeSuffix = isDarkTheme
                    ? "Dark"
                    : "Light";

                colorKey += themeSuffix;
            }

            return Current?.Resources != null &&
                Current.Resources.TryGetValue(colorKey, out object value) &&
                value is Color color
                ? color.ToHex()
                : "#000000";
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
                var userInterface = DependencyService.Get<IUserInterface>();
                userInterface.UserAppTheme = Settings.AppTheme;
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

            var geolocationService = DependencyService.Get<IGeolocationService>();

            if (geolocationService.IsListening)
            {
                this.restartLocationUpdates = true;

                geolocationService.StopListening();
            }
        }

        /// <summary>
        /// Called when application is resumed
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();

            if (Settings != null)
            {
                var userInterface = DependencyService.Get<IUserInterface>();
                userInterface.UserAppTheme = Settings.AppTheme;
            }

            var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveWaypointRefreshService.ResumeTimer();

            if (this.restartLocationUpdates)
            {
                this.restartLocationUpdates = false;

                Task.Run(async () =>
                {
                    var geolocationService = DependencyService.Get<IGeolocationService>();
                    await geolocationService.StartListeningAsync();
                });
            }
        }
        #endregion
    }
}
