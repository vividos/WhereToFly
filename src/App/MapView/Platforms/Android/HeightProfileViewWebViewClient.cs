using Android.Content;
using Android.Webkit;
using AndroidX.WebKit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System.Diagnostics;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// WebViewClient implementation for Android
    /// </summary>
    internal class HeightProfileViewWebViewClient : MauiWebViewClient
    {
        /// <summary>
        /// Asset loader
        /// </summary>
        private readonly WebViewAssetLoader assetLoader;

        /// <summary>
        /// Creates a new web view client
        /// </summary>
        /// <param name="context">context where the Android's WebView is running on</param>
        /// <param name="handler">web view handler</param>
        public HeightProfileViewWebViewClient(
            Context context,
            WebViewHandler handler)
            : base(handler)
        {
            this.assetLoader = new WebViewAssetLoader
                .Builder()
                .AddPathHandler(
                    "/assets/",
                    new WebViewAssetLoader.AssetsPathHandler(context))
                .Build();
        }

        /// <summary>
        /// Called when an error occured when receiving an URL; just forwards call to previous
        /// client.
        /// </summary>
        /// <param name="view">web view</param>
        /// <param name="request">web resource request that failed</param>
        /// <param name="error">web resource error</param>
        [System.Runtime.Versioning.SupportedOSPlatform("android23.0")]
        public override void OnReceivedError(
            WebView? view,
            IWebResourceRequest? request,
            WebResourceError? error)
        {
            string errorDescription = error?.DescriptionFormatted?.ToString() ?? "N/A";

            Debug.WriteLine(
                "OnReceivedError: method={0} url={1}, error={2}",
                request?.Method,
                request?.Url?.ToString() ?? "N/A",
                errorDescription);

            base.OnReceivedError(view, request, error);
        }

        /// <summary>
        /// Checks if the given web resource request should be intercepted and returns a different
        /// web resource response. Note that this works starting from Android 5.0 (API level 21).
        /// </summary>
        /// <param name="view">web view</param>
        /// <param name="request">web resource request to inspect</param>
        /// <returns>new response</returns>
        public override WebResourceResponse? ShouldInterceptRequest(
            WebView? view,
            IWebResourceRequest? request)
        {
            Debug.WriteLine(
                "ShouldInterceptRequest: method={0} url={1}",
                request?.Method,
                request?.Url?.ToString());

            if (request?.Url == null)
            {
                return null;
            }

            return this.assetLoader.ShouldInterceptRequest(request.Url);
        }
    }
}
