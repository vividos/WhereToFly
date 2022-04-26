using System;
using System.IO;
using Xamarin.Forms;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Web view source factory implementation for Android
    /// </summary>
    public partial class WebViewSourceFactoryImpl : IWebViewSourceFactory
    {
        /// <summary>
        /// Base path to use in WebView control, for Android
        /// </summary>
        public string WebViewBasePath => "file:///android_asset/weblib/";

        /// <summary>
        /// Loads text of Android asset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>text content of asset</returns>
        private string LoadAssetText(string assetFilename)
        {
            var assetManager = Android.App.Application.Context.Assets;
            if (assetManager == null)
            {
                throw new InvalidOperationException("can't access Android asset manager");
            }

            using var stream = assetManager.Open(assetFilename);
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Returns HTML web view source for MapView
        /// </summary>
        /// <returns>web view source</returns>
        public WebViewSource GetMapViewSource()
        {
            return new HtmlWebViewSource
            {
                Html = this.LoadAssetText("weblib/mapView.html"),
                BaseUrl = this.WebViewBasePath,
            };
        }

        /// <summary>
        /// Returns HTML web view source for HeightProfileView
        /// </summary>
        /// <returns>web view source</returns>
        public WebViewSource GetHeightProfileViewSource()
        {
            return new HtmlWebViewSource
            {
                Html = this.LoadAssetText("weblib/heightProfileView.html"),
                BaseUrl = this.WebViewBasePath,
            };
        }
    }
}
