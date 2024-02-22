using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;

namespace WhereToFly.App.Maui
{
    /// <summary>
    /// MAUI app
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Creates a new MAUI app object
        /// </summary>
        public App()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android ||
                DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                AppCenter.Start(
                    $"android={Constants.AppCenterKeyAndroid};" +
                    $"uwp={Constants.AppCenterKeyWindows}",
                    typeof(Distribute),
                    typeof(Crashes));
            }

            TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;

            this.InitializeComponent();

            this.MainPage = new NavigationPage(new MainPage())
            {
                BarBackgroundColor = Color.FromRgb(0x2f, 0x29, 0x9e),
            };
        }

        /// <summary>
        /// Called when the app's window is about to be created. Sets the app's title.
        /// </summary>
        /// <param name="activationState">activation state</param>
        /// <returns>window object</returns>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);
            window.Title = "Where-to-fly";

            return window;
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
        /// Logs exception that is occured and was caught, but is not presented to the user. Send
        /// it to AppCenter for further analysis.
        /// </summary>
        /// <param name="ex">exception to log</param>
        public static void LogError(Exception ex)
        {
            Crashes.TrackError(ex);
        }
    }
}
