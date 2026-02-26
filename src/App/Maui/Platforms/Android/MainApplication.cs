using Android.App;
using Android.Runtime;
using Android.Webkit;

namespace WhereToFly.App
{
    /// <summary>
    /// Android MAUI app application
    /// </summary>
    [Application]
    public class MainApplication : MauiApplication
    {
        /// <summary>
        /// Creates a new MAUI application object
        /// </summary>
        /// <param name="handle">Android handle</param>
        /// <param name="ownership">handle ownership</param>
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        /// <summary>
        /// Called when the Android app is about to be created
        /// </summary>
        public override void OnCreate()
        {
            base.OnCreate();

            Task.Run(() =>
            {
                // start WebView initialization early
                _ = WebSettings.GetDefaultUserAgent(this.ApplicationContext);
            });
        }

        /// <summary>
        /// Creates a new MAUI app
        /// </summary>
        /// <returns>MAUI app</returns>
        protected override MauiApp CreateMauiApp()
            => MauiProgram.CreateMauiApp();
    }
}
