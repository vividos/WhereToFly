using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.Styles;
using WhereToFly.App.Core.Views;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.Essentials;
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
        /// Indicates if listening to location updates should be restarted when resuming the app
        /// </summary>
        private bool restartLocationUpdates;

        /// <summary>
        /// Application settings; this is initialized when <see cref="InitializedTask"/> has
        /// completed, and is available in every content page.
        /// </summary>
        public static AppSettings? Settings { get; internal set; }

        /// <summary>
        /// The one and only map page (displaying the map using CesiumJS)
        /// </summary>
        public MapPage? MapPage { get; internal set; }

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
            if (DeviceInfo.Platform == DevicePlatform.Android ||
                DeviceInfo.Platform == DevicePlatform.UWP)
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
            DependencyService.Register<IAppMapService, AppMapService>();
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
            this.MainPage = new RootPage();
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

            var appMapService = DependencyService.Get<IAppMapService>();
            await appMapService.InitLiveWaypointRefreshService();

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
        public static async Task RunOnUiThreadAsync(Action action)
        {
            await Xamarin.Forms.Device.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    App.LogError(ex);
                }
            });
        }

        /// <summary>
        /// Shows toast message with given text
        /// </summary>
        /// <param name="message">toast message text</param>
        public static void ShowToast(string message)
        {
            MainThread.BeginInvokeOnMainThread(
                () => App.Current.MainPage.DisplayToastAsync(
                    new ToastOptions
                    {
                        MessageOptions = new MessageOptions
                        {
                            Message = message,
                            Foreground = Color.White,
                        },
                        BackgroundColor = Constants.PrimaryColor,
                    }));
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

            var geolocationService = DependencyService.Get<IGeolocationService>();

            if (geolocationService.IsListening)
            {
                this.restartLocationUpdates = true;

                Task.Run(async () =>
                {
                    await geolocationService.StopListeningAsync();
                });
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
                ThemeHelper.ChangeTheme(Settings.AppTheme, true);
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
