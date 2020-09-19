using System.IO;
using Xamarin.UITest;

namespace WhereToFly.App.UITest
{
    /// <summary>
    /// App initializer for unit tests; initializes IApp instance, depending on the platform.
    /// </summary>
    public static class AppInitializer
    {
        /// <summary>
        /// Starts up app for the given platform
        /// </summary>
        /// <param name="platform">platform to start app for</param>
        /// <returns>started app</returns>
        public static IApp StartApp(Platform platform)
        {
            if (platform == Platform.Android)
            {
                string apkFilename = Path.Combine(
                    Path.GetDirectoryName(typeof(AppInitializer).Assembly.Location),
                    "de.vividos.app.wheretofly.android.apk");

                return
                    ConfigureApp.Android
                    .ApkFile(apkFilename)
                    .EnableLocalScreenshots()
                    .StartApp();
            }

            return ConfigureApp.iOS.StartApp();
        }
    }
}
