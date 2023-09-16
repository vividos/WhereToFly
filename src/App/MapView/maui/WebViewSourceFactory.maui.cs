using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.IO;
using System.Threading.Tasks;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Web view source factory implementation for MAUI
    /// </summary>
    internal partial class WebViewSourceFactory
    {
        /// <summary>
        /// Base URL for WebLib library in WebView
        /// </summary>
#pragma warning disable S1075 // URIs should not be hardcoded
#if WINDOWS
        private const string WebLibWebViewBaseUrl = "https://appdir/weblib/";
#elif ANDROID
        private const string WebLibWebViewBaseUrl = "file:///android_asset/weblib/";
#endif
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// Loads text of MauiAsset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>text content of asset</returns>
        private static async Task<string> LoadAssetText(string assetFilename)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(assetFilename);

            if (stream == null)
            {
                return null;
            }

            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Returns HTML web view source for MapView
        /// </summary>
        /// <returns>web view source</returns>
        public async Task<WebViewSource> PlatformGetMapViewSource()
        {
            return new HtmlWebViewSource
            {
                Html = await LoadAssetText("weblib/mapView.html"),
                BaseUrl = WebLibWebViewBaseUrl,
            };
        }

        /// <summary>
        /// Returns HTML web view source for HeightProfileView
        /// </summary>
        /// <returns>web view source</returns>
        public async Task<WebViewSource> PlatformGetHeightProfileViewSource()
        {
            return new HtmlWebViewSource
            {
                Html = await LoadAssetText("weblib/heightProfileView.html"),
                BaseUrl = WebLibWebViewBaseUrl,
            };
        }
    }
}
