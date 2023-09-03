using Android.App;
using Android.Runtime;

namespace WhereToFly.App.Maui
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
        /// Creates a new MAUI app
        /// </summary>
        /// <returns>MAUI app</returns>
        protected override MauiApp CreateMauiApp()
            => MauiProgram.CreateMauiApp();
    }
}
