using Microsoft.Maui.Controls;
using System.Threading.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

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
        private const string WebLibWebViewBaseUrl = "https://appassets.androidplatform.net/assets/weblib/";
#endif
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// Returns HTML web view source for MapView
        /// </summary>
        /// <returns>web view source</returns>
        public async Task<WebViewSource> PlatformGetMapViewSource()
        {
            return new UrlWebViewSource
            {
                Url = WebLibWebViewBaseUrl + "mapView.html",
            };
        }

        /// <summary>
        /// Returns HTML web view source for HeightProfileView
        /// </summary>
        /// <returns>web view source</returns>
        public async Task<WebViewSource> PlatformGetHeightProfileViewSource()
        {
            return new UrlWebViewSource
            {
                Url = WebLibWebViewBaseUrl + "heightProfileView.html",
            };
        }
    }
}
