using System;
using System.IO;
using WhereToFly.App.Core;
using Windows.Storage;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(WhereToFly.App.UWP.UwpPlatform))]

namespace WhereToFly.App.UWP
{
    /// <summary>
    /// Platform specific functions
    /// </summary>
    public class UwpPlatform : IPlatform
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        /// <summary>
        /// Path URL for assets files
        /// </summary>
        private const string AppxAssetsPathUrl1 = "ms-appx:///WhereToFly.App.Resources/Assets/";

        /// <summary>
        /// Path URL for assets files, second variant
        /// </summary>
        private const string AppxAssetsPathUrl2 = "ms-appx:///WhereToFly.App.MapView/Assets/";
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// Base path to use in WebView control, for UWP
        /// </summary>
        public string WebViewBasePath => "ms-appx-web:///WhereToFly.App.MapView/Assets/";

        /// <summary>
        /// Opens UWP asset stream and returns it
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>stream to read from file</returns>
        public Stream OpenAssetStream(string assetFilename)
        {
            string fullAssetPath = AppxAssetsPathUrl1 + assetFilename;
            var uri = new Uri(fullAssetPath);

            StorageFile file = null;
            try
            {
                file = StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().Result;
            }
            catch (Exception)
            {
            }

            if (file == null)
            {
                fullAssetPath = AppxAssetsPathUrl2 + assetFilename;
                uri = new Uri(fullAssetPath);
                file = StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().Result;
            }

            return file.OpenStreamForReadAsync().Result;
        }

        /// <summary>
        /// Loads text of UWP asset file from given filename
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

        /// <summary>
        /// Sets app theme to use for platform. This ensures that platform dependent dialogs are
        /// themed correctly when switching themes.
        /// </summary>
        /// <param name="requestedTheme">requested theme</param>
        public void SetPlatformTheme(OSAppTheme requestedTheme)
        {
            // switch to UI thread; or else accessing RequestedTheme on UWP crashes
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => this.SetPlatformTheme(requestedTheme));
                return;
            }

            try
            {
                switch (requestedTheme)
                {
                    case OSAppTheme.Dark: App.Current.RequestedTheme = Windows.UI.Xaml.ApplicationTheme.Dark; break;
                    case OSAppTheme.Light: App.Current.RequestedTheme = Windows.UI.Xaml.ApplicationTheme.Light; break;
                    default:
                        // ignore other requested themes
                        break;
                }
            }
            catch (Exception)
            {
                // ignore errors when setting theme
            }
        }
    }
}
