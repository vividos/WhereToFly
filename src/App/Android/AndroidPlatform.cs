using AndroidX.AppCompat.App;
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
        /// Base path to use in WebView control, for Android
        /// </summary>
        public string WebViewBasePath => "file:///android_asset/";

        /// <summary>
        /// Opens asset stream and returns it
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>stream to read from file</returns>
        public Stream OpenAssetStream(string assetFilename)
        {
            var assetManager = global::Android.App.Application.Context.Assets;

            return assetManager.Open(assetFilename);
        }

        /// <summary>
        /// Loads text of Android asset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>text content of asset</returns>
        public string LoadAssetText(string assetFilename)
        {
            using var stream = this.OpenAssetStream(assetFilename);
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Sets app theme to use for platform. This ensures that platform dependent dialogs are
        /// themed correctly when switching themes.
        /// </summary>
        /// <param name="requestedTheme">requested theme</param>
        public void SetPlatformTheme(OSAppTheme requestedTheme)
        {
            AppCompatDelegate.DefaultNightMode = requestedTheme switch
            {
                OSAppTheme.Dark => AppCompatDelegate.ModeNightYes,
                OSAppTheme.Light => AppCompatDelegate.ModeNightNo,
                _ => AppCompatDelegate.ModeNightFollowSystem,
            };
        }
    }
}
