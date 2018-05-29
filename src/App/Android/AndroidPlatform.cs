using Android.Content;
using Plugin.CurrentActivity;
using System.IO;
using WhereToFly.App.Core;
using Xamarin.Forms;

[assembly: Dependency(typeof(WhereToFly.App.Android.AndroidPlatform))]

namespace WhereToFly.App.Android
{
    /// <summary>
    /// Platform specific functions
    /// </summary>
    public class AndroidPlatform : IPlatform
    {
        /// <summary>
        /// Returns current context, either from current activity, or the global application
        /// context
        /// </summary>
        private static Context CurrentContext
        {
            get
            {
                return CrossCurrentActivity.Current.Activity ?? global::Android.App.Application.Context;
            }
        }

        /// <summary>
        /// Property containing the app version number
        /// </summary>
        public string AppVersionNumber
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;

                string versionText = string.Format(
                    "{0}.{1}.{2} (Build {3})",
                    version.Major,
                    version.Minor,
                    version.Build,
                    version.Revision);

                return versionText;
            }
        }

        /// <summary>
        /// Property containing the Android app data folder
        /// </summary>
        public string AppDataFolder
        {
            get
            {
                return CurrentContext.FilesDir.AbsolutePath;
            }
        }

        /// <summary>
        /// Property containing the Android cache data folder
        /// </summary>
        public string CacheDataFolder
        {
            get
            {
                return CurrentContext.CacheDir.AbsolutePath;
            }
        }

        /// <summary>
        /// Base path to use in WebView control, for Android
        /// </summary>
        public string WebViewBasePath
        {
            get { return "file:///android_asset/"; }
        }

        /// <summary>
        /// Opens asset stream and returns it
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>stream to read from file</returns>
        public Stream OpenAssetStream(string assetFilename)
        {
            var assetManager = CurrentContext.Assets;

            return assetManager.Open(assetFilename);
        }

        /// <summary>
        /// Loads text of Android asset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>text content of asset</returns>
        public string LoadAssetText(string assetFilename)
        {
            using (var stream = this.OpenAssetStream(assetFilename))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
