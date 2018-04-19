using Android.Runtime;
using Android.Webkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace WhereToFly.App.Android
{
    /// <summary>
    /// WebViewClient implementation for Android
    /// </summary>
    internal class AndroidWebViewClient : WebViewClient
    {
        /// <summary>
        /// HTTP client to use to download content when intercepting content
        /// </summary>
        private readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Previous web view client in the chain
        /// </summary>
        private WebViewClient previousClient;

        /// <summary>
        /// List of website host addresses (or parts) where CORS headers should be replaced in
        /// order for the WebView to correctly receive content. This must be used in some cases,
        /// since the default Base URL of the WebView is file://
        /// </summary>
        public HashSet<string> CorsWebsiteHosts { get; set; } = new HashSet<string>();

        /// <summary>
        /// Creates a new web view client
        /// </summary>
        /// <param name="previousClient">
        /// previous web view client; calls are forwarded to this client
        /// </param>
        public AndroidWebViewClient(WebViewClient previousClient)
        {
            this.previousClient = previousClient;
        }

        /// <summary>
        /// Called when the page has finished loading
        /// </summary>
        /// <param name="view">web view</param>
        /// <param name="url">page URL</param>
        public override void OnPageFinished(WebView view, string url)
        {
            this.previousClient.OnPageFinished(view, url);
        }

        /// <summary>
        /// Called when an error occured when receiving an URL; just forwards call to previous
        /// client.
        /// </summary>
        /// <param name="view">web view</param>
        /// <param name="errorCode">error code</param>
        /// <param name="description">error description</param>
        /// <param name="failingUrl">failing URL</param>
        [Obsolete]
        public override void OnReceivedError(WebView view, [GeneratedEnum] ClientError errorCode, string description, string failingUrl)
        {
            this.previousClient.OnReceivedError(view, errorCode, description, failingUrl);
        }

        /// <summary>
        /// Called when an error occured when receiving an URL; just forwards call to previous
        /// client.
        /// </summary>
        /// <param name="view">web view</param>
        /// <param name="request">web resource request that failed</param>
        /// <param name="error">web resource error</param>
        public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
        {
            Debug.WriteLine(
                "OnReceivedError: method={0} url={1}, error={2}",
                request.Method,
                request.Url.ToString(),
                error.DescriptionFormatted.ToString());

            this.previousClient.OnReceivedError(view, request, error);
        }

        /// <summary>
        /// Called in order to check if loading a resource from an URL should be overridden; just
        /// forwards call to previous client.
        /// </summary>
        /// <param name="view">web view</param>
        /// <param name="url">URL to be checked</param>
        /// <returns>what previous client returns</returns>
        [Obsolete]
        public override bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            return this.previousClient.ShouldOverrideUrlLoading(view, url);
        }

        /// <summary>
        /// Checks if the given web resource request should be intercepted and returns a different
        /// web resource response. Note that this works starting from Android 5.0 (API level 21).
        /// See https://stackoverflow.com/questions/17272612/android-webview-disable-cors
        /// </summary>
        /// <param name="view">web view</param>
        /// <param name="request">web resource request to inspect</param>
        /// <returns>new response</returns>
        public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
        {
            Debug.WriteLine(
                "ShouldInterceptRequest: method={0} url={1}",
                request.Method,
                request.Url.ToString());

            string host = request.Url.Host.ToLowerInvariant();

            if (this.CorsWebsiteHosts.Any(x => host.Contains(x)))
            {
                return this.BuildCorsResponse(request.Url.ToString());
            }

            return this.previousClient.ShouldInterceptRequest(view, request);
        }

        /// <summary>
        /// Builds a new web resource response that circumvents the CORS headers sent while
        /// fetching the URL content using HttpClient.
        /// </summary>
        /// <param name="url">URL of web resource to get</param>
        /// <returns>newly created web resource response</returns>
        private WebResourceResponse BuildCorsResponse(string url)
        {
            string date = DateTime.Now.ToUniversalTime().ToString("r");
            string domainName = "file://";

            var responseHeaders = new Dictionary<string, string>()
            {
                { "Connection", "close" },
                { "Content-Type", "text/plain" },
                { "Date", date },
                { "Access-Control-Allow-Origin", domainName },
                { "Access-Control-Allow-Methods", "GET, POST, DELETE, PUT, OPTIONS" },
                { "Access-Control-Max-Age", "600" },
                { "Access-Control-Allow-Credentials", "true" },
                { "Access-Control-Allow-Headers", "accept, authorization, Content-Type" },
                { "Via", "1.1 vegur" },
            };

            Stream stream = null;
            try
            {
                stream = this.httpClient.GetStreamAsync(url).Result;
            }
            catch (Exception)
            {
                // ignore exceptions when fetching content (and use null stream)
            }

            return new WebResourceResponse(
                "text/plain",
                "UTF-8",
                200,
                "OK",
                responseHeaders,
                stream);
        }

        /// <summary>
        /// Disposes of managed and unmanaged resources
        /// </summary>
        /// <param name="disposing">
        /// true when called from Dispose(), false when called from finalizer
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.previousClient = null;

                if (this.httpClient != null)
                {
                    this.httpClient.Dispose();
                }
            }
        }
    }
}
