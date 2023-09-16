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
            string text = await reader.ReadToEndAsync();

#if NET7_0
            // Workaround: The MAUI WebView implementation fails to set the base URL when there's
            // already a base tag; as a quick fix, replace it here
            text = text.Replace("<base ", "<base href='" + WebLibWebViewBaseUrl + "' ");
#endif

            return text;
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
#if NET7_0 && WINDOWS
                // Working around another bug: setting the BaseUrl using the LocalScheme
                // URL fails to set up the virtual folder mapping, so just pass null here.
                // See: https://github.com/dotnet/maui/issues/16646
                BaseUrl = null,
#else
                BaseUrl = WebLibWebViewBaseUrl,
#endif
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
#if NET7_0 && WINDOWS
                // Working around another bug: setting the BaseUrl using the LocalScheme
                // URL fails to set up the virtual folder mapping, so just pass null here.
                // See: https://github.com/dotnet/maui/issues/16646
                BaseUrl = null,
#else
                BaseUrl = WebLibWebViewBaseUrl,
#endif
            };
        }
    }
}
