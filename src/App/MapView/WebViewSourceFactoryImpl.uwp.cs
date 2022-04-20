using Xamarin.Forms;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Web view source factory implementation for UWP
    /// </summary>
    public partial class WebViewSourceFactoryImpl : IWebViewSourceFactory
    {
        /// <summary>
        /// Base path to use in WebView control, for UWP
        /// </summary>
        public string WebViewBasePath => "ms-appx-web:///WhereToFly.App.MapView/Assets/";

        /// <summary>
        /// Returns URL web view source for MapView
        /// </summary>
        /// <returns>web view source</returns>
        public WebViewSource GetMapViewSource()
        {
            return new UrlWebViewSource
            {
                Url = this.WebViewBasePath + "weblib/mapView.html",
            };
        }

        /// <summary>
        /// Returns URL web view source for HeightProfileView
        /// </summary>
        /// <returns>web view source</returns>
        public WebViewSource GetHeightProfileViewSource()
        {
            return new UrlWebViewSource
            {
                Url = this.WebViewBasePath + "weblib/heightProfileView.html",
            };
        }
    }
}
