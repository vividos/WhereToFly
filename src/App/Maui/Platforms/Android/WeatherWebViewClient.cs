using Android.Webkit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using WebView = global::Android.Webkit.WebView;

namespace WhereToFly.App
{
    /// <summary>
    /// This <see cref="WebViewClient"/> for the WeatherWebView control ensures that external
    /// links clicked on the weather web pages, or page redirects, are again opened in the same
    /// web view by overriding ShouldOverrideUrlLoading() and returning <see langword="false"/>
    /// for https links. See also:
    /// https://stackoverflow.com/questions/4066438/android-webview-how-to-handle-redirects-in-app-instead-of-opening-a-browser
    /// </summary>
    internal class WeatherWebViewClient : MauiWebViewClient
    {
        /// <summary>
        /// Creates a new web view client
        /// </summary>
        /// <param name="handler">web view handler</param>
        public WeatherWebViewClient(WebViewHandler handler)
            : base(handler)
        {
        }

        /// <summary>
        /// Called when the user clicked on a link; this method decides if the link should be
        /// opened in the same web view or externally.
        /// https://stackoverflow.com/questions/4066438/android-webview-how-to-handle-redirects-in-app-instead-of-opening-a-browser
        /// </summary>
        /// <param name="view">web view</param>
        /// <param name="request">request</param>
        /// <returns>false when URL should be loaded in the same web view</returns>
        public override bool ShouldOverrideUrlLoading(
            WebView? view,
            IWebResourceRequest? request)
        {
            // always open normal web links in the WebView
#pragma warning disable S1125 // Boolean literals should not be redundant
            return request?.Url?.Scheme == "https"
                ? false
                : base.ShouldOverrideUrlLoading(view, request);
#pragma warning restore S1125 // Boolean literals should not be redundant
        }
    }
}
