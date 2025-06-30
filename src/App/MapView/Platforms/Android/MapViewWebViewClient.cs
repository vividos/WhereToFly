using Android.Content;
using Android.Webkit;
using AndroidX.WebKit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Diagnostics;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// WebViewClient implementation for Android
    /// </summary>
    internal class MapViewWebViewClient : MauiWebViewClient
    {
        /// <summary>
        /// Action to log errors
        /// </summary>
        private readonly Action<Exception>? logErrorAction;

        /// <summary>
        /// Asset loader
        /// </summary>
        private readonly WebViewAssetLoader assetLoader;

        /// <summary>
        /// CORS website cache
        /// </summary>
        private readonly CorsWebsiteCache corsWebsiteCache = new();

        /// <summary>
        /// Creates a new web view client
        /// </summary>
        /// <param name="context">context where the Android's WebView is running on</param>
        /// <param name="handler">web view handler</param>
        /// <param name="logErrorAction">action to log errors</param>
        public MapViewWebViewClient(
            Context context,
            WebViewHandler handler,
            Action<Exception>? logErrorAction)
            : base(handler)
        {
            this.logErrorAction = logErrorAction;

            this.corsWebsiteCache.CorsWebsiteHosts.Add("thermal.kk7.ch");

            this.assetLoader =
                new WebViewAssetLoader.Builder()
                .AddPathHandler(
                    "/assets/",
                    new WebViewAssetLoader.AssetsPathHandler(context))
                ?.Build()
                ?? throw new InvalidOperationException("must be able to build a WebViewAssetLoader");
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
        /// See https://stackoverflow.com/questions/17272612/android-webview-disable-cors
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

            var corsResponse = this.corsWebsiteCache.ShouldInterceptRequest(request);
            if (corsResponse != null)
            {
                return corsResponse;
            }

            // AppCenter infrequently reports an exception; try to fix this
            try
            {
                return this.assetLoader.ShouldInterceptRequest(request.Url);
            }
            catch (Exception ex)
            {
                this.logErrorAction?.Invoke(ex);
                return null;
            }
        }
    }
}
