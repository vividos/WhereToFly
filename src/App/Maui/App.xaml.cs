using System.Diagnostics;
using WhereToFly.App.Abstractions;
using WhereToFly.App.Logic;
using WhereToFly.App.Models;
using WhereToFly.App.Pages;
using WhereToFly.App.Services;
using WhereToFly.Shared.Model;

#if APPCENTER
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
#endif

#if WINDOWS
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
#endif

// make app internals visible to unit tests
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WhereToFly.App.UnitTest")]

namespace WhereToFly.App
{
    /// <summary>
    /// WhereToFly app
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Task that can be awaited to wait for a completed app initialisation. The task performs
        /// the following:
        /// - sets up dependency service objects
        /// - sets up main page and map page
        /// - loads app data
        /// - initializes live waypoint refresh service
        /// Note that MapPage also has a task to wait for initialized page.
        /// </summary>
        public static Task InitializedTask { get; internal set; } = Task.CompletedTask;

        /// <summary>
        /// Service provider
        /// </summary>
        public static IServiceProvider Services
            => IPlatformApplication.Current?.Services
            ?? throw new InvalidOperationException("IServiceProvider is not available");

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
        public App()
        {
#if APPCENTER && (ANDROID || WINDOWS)
            AppCenter.Start(
                $"android={Constants.AppCenterKeyAndroid};" +
                $"windowsdesktop={Constants.AppCenterKeyWindows}",
                typeof(Crashes));
#endif

            TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;

            this.InitializeComponent();

            SetupDepencencyService();

            App.InitializedTask = Task.Run(this.LoadAppDataAsync);

            this.RequestedThemeChanged += this.OnRequestedThemeChanged;
        }

        /// <summary>
        /// Called when the app's window is about to be created. Sets the app's title.
        /// </summary>
        /// <param name="activationState">activation state</param>
        /// <returns>window object</returns>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var titleBar = DeviceInfo.Platform == DevicePlatform.WinUI
                ? new TitleBar
                {
                    Title = Constants.AppTitle,
                    BackgroundColor = Constants.PrimaryColor,
                    ForegroundColor = Colors.White,
                }
                : null;

            return new Window
            {
                Title = Constants.AppTitle,
                TitleBar = titleBar,
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
                var userInterface = Services.GetRequiredService<IUserInterface>();
                userInterface.UserAppTheme = Settings.AppTheme;
            }
        }

        /// <summary>
        /// Sets up DependencyService
        /// </summary>
        private static void SetupDepencencyService()
        {
            DependencyService.RegisterSingleton(Services.GetRequiredService<IUserInterface>());
            DependencyService.RegisterSingleton(Services.GetRequiredService<IAppMapService>());
            DependencyService.RegisterSingleton(Services.GetRequiredService<INavigationService>());
            DependencyService.RegisterSingleton(Services.GetRequiredService<IDataService>());
            DependencyService.RegisterSingleton(Services.GetRequiredService<IGeolocationService>());
            DependencyService.RegisterSingleton(Services.GetRequiredService<LiveDataRefreshService>());
        }

        /// <summary>
        /// Loads app data needed for running, e.g. app settings, caches and location list for
        /// live waypoint refresh services.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task LoadAppDataAsync()
        {
            var dataService = Services.GetRequiredService<IDataService>();
            Config = await dataService.GetAppConfigAsync(CancellationToken.None);
            Settings = await dataService.GetAppSettingsAsync(CancellationToken.None);

            var userInterface = Services.GetRequiredService<IUserInterface>();
            userInterface.UserAppTheme = Settings.AppTheme;

            var appMapService = Services.GetRequiredService<IAppMapService>();
            await appMapService.InitLiveWaypointRefreshService();
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
            Debug.WriteLine($"UnobservedTaskException: {args.Exception}");

            LogError(args.Exception);
        }

        /// <summary>
        /// Logs exception that is occured and was caught, but is not presented to the user. Send
        /// it to AppCenter for further analysis.
        /// </summary>
        /// <param name="ex">exception to log</param>
        public static void LogError(Exception ex)
        {
            if (ex is AggregateException aggregateException &&
                aggregateException.InnerExceptions.Count == 1 &&
                aggregateException.InnerException != null)
            {
                ex = aggregateException.InnerException;
            }

#if APPCENTER
            Crashes.TrackError(ex);
#else
            Debug.WriteLine($"App exception occurred: {ex}");
#endif
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

        /// <summary>
        /// Called when a file is dropped on the app's window
        /// </summary>
        /// <param name="args">drop event args</param>
        /// <returns>task to wait on</returns>
        public static async Task OnDropFile(DropEventArgs? args)
        {
#if WINDOWS
            var dataView = args?.PlatformArgs?.DragEventArgs.DataView;

            if (dataView == null ||
                !dataView.Contains(StandardDataFormats.StorageItems))
            {
                return;
            }

            var items = await dataView.GetStorageItemsAsync();

            if (items.Count > 0 &&
                items[0] is StorageFile file &&
                !string.IsNullOrEmpty(file.Name))
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    using var stream = await file.OpenStreamForReadAsync();
                    await OpenFileHelper.OpenFileAsync(stream, file.Name);
                });
            }
#else
            await Task.CompletedTask;
#endif
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
                var userInterface = Services.GetRequiredService<IUserInterface>();
                userInterface.UserAppTheme = Settings.AppTheme;
            }
        }

        /// <summary>
        /// Called when application is paused
        /// </summary>
        protected override void OnSleep()
        {
            base.OnSleep();

            var liveWaypointRefreshService = Services.GetRequiredService<LiveDataRefreshService>();
            liveWaypointRefreshService.StopTimer();

            var geolocationService = Services.GetRequiredService<IGeolocationService>();

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
                var userInterface = Services.GetRequiredService<IUserInterface>();
                userInterface.UserAppTheme = Settings.AppTheme;
            }

            var liveWaypointRefreshService = Services.GetRequiredService<LiveDataRefreshService>();
            liveWaypointRefreshService.ResumeTimer();

            if (this.restartLocationUpdates)
            {
                this.restartLocationUpdates = false;

                Task.Run(async () =>
                {
                    var geolocationService = Services.GetRequiredService<IGeolocationService>();
                    await geolocationService.StartListeningAsync();
                });
            }
        }
        #endregion
    }
}
