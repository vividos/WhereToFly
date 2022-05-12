using System.Threading.Tasks;
using Xamarin.Forms;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Web view source factory implementation for UWP
    /// </summary>
    internal partial class WebViewSourceFactory
    {
        /// <summary>
        /// Base path to use in WebView control, for UWP
        /// </summary>
#pragma warning disable S1075 // URIs should not be hardcoded
        private const string WebViewBasePath = "ms-appx-web:///WhereToFly.App.MapView/Assets/";
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// Returns URL web view source for MapView
        /// </summary>
        /// <returns>web view source</returns>
        public Task<WebViewSource> PlatformGetMapViewSource()
        {
            return Task.FromResult<WebViewSource>(
                new UrlWebViewSource
                {
                    Url = WebViewBasePath + "weblib/mapView.html",
                });
        }

        /// <summary>
        /// Returns URL web view source for HeightProfileView
        /// </summary>
        /// <returns>web view source</returns>
        public Task<WebViewSource> PlatformGetHeightProfileViewSource()
        {
            return Task.FromResult<WebViewSource>(
                new UrlWebViewSource
                {
                    Url = WebViewBasePath + "weblib/heightProfileView.html",
                });
        }
    }
}
