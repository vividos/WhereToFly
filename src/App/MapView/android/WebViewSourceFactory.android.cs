using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Web view source factory implementation for Android
    /// </summary>
    internal partial class WebViewSourceFactory
    {
        /// <summary>
        /// Base path to use in WebView control, for Android
        /// </summary>
#pragma warning disable S1075 // URIs should not be hardcoded
        private const string WebLibWebViewBaseUrl = "https://appassets.androidplatform.net/assets/weblib/";
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// Loads text of Android asset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>text content of asset</returns>
        private async Task<string> LoadAssetText(string assetFilename)
        {
            var assetManager = Android.App.Application.Context.Assets;
            if (assetManager == null)
            {
                throw new InvalidOperationException("can't access Android asset manager");
            }

            using var stream = assetManager.Open(assetFilename);
            using var streamReader = new StreamReader(stream);
            return await streamReader.ReadToEndAsync();
        }

        /// <summary>
        /// Returns web view source for MapView
        /// </summary>
        /// <returns>web view source</returns>
        public Task<WebViewSource> PlatformGetMapViewSource()
        {
            return Task.FromResult<WebViewSource>(new UrlWebViewSource
            {
                Url = WebLibWebViewBaseUrl + "mapView.html",
            });
        }

        /// <summary>
        /// Returns web view source for HeightProfileView
        /// </summary>
        /// <returns>web view source</returns>
        public Task<WebViewSource> PlatformGetHeightProfileViewSource()
        {
            return Task.FromResult<WebViewSource>(new UrlWebViewSource
            {
                Url = WebLibWebViewBaseUrl + "heightProfileView.html",
            });
        }
    }
}
