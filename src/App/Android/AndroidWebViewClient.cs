using Android.Webkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using Xamarin.Essentials;
using Xamarin.Forms.Platform.Android;

namespace WhereToFly.App.Android
{
    /// <summary>
    /// WebViewClient implementation for Android
    /// </summary>
    internal class AndroidWebViewClient : FormsWebViewClient
    {
        /// <summary>
        /// HTTP client to use to download content when intercepting content
        /// </summary>
        private readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Cache folder name for CORS content
        /// </summary>
        private readonly string cacheFolder;

        /// <summary>
        /// Number of cache hits
        /// </summary>
        private int numCacheHit;

        /// <summary>
        /// Number of cache misses
        /// </summary>
        private int numCacheMiss;

        /// <summary>
        /// List of website host addresses (or parts) where CORS headers should be replaced in
        /// order for the WebView to correctly receive content. This must be used in some cases,
        /// since the default Base URL of the WebView is file://
        /// </summary>
        public HashSet<string> CorsWebsiteHosts { get; set; } = new HashSet<string>();

        /// <summary>
        /// Creates a new web view client
        /// </summary>
        /// <param name="renderer">web view renderer</param>
        public AndroidWebViewClient(WebViewRenderer renderer)
            : base(renderer)
        {
            this.cacheFolder = GetCacheFolder();
        }

        /// <summary>
        /// Constructor needed when the web view client is copied by Java
        /// </summary>
        /// <param name="javaReference">java reference</param>
        /// <param name="transfer">ownership transfer value</param>
        protected AndroidWebViewClient(IntPtr javaReference, global::Android.Runtime.JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            this.cacheFolder = GetCacheFolder();
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
        public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
        {
            Debug.WriteLine(
                "ShouldInterceptRequest: method={0} url={1}",
                request.Method,
                request.Url?.ToString());

            string host = request?.Url?.Host?.ToLowerInvariant();

            if (host != null &&
                this.CorsWebsiteHosts != null &&
                this.CorsWebsiteHosts.Any(x => host.Contains(x)))
            {
                var response = this.BuildCorsResponse(request.Url.ToString());
                if (response != null)
                {
                    return response;
                }
            }

            // AppCenter infrequently reports an exception; try to fix this
            try
            {
                return base.ShouldInterceptRequest(view, request);
            }
            catch (Exception ex)
            {
                Core.App.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// Builds a new web resource response that circumvents the CORS headers sent while
        /// fetching the URL content using HttpClient.
        /// </summary>
        /// <param name="url">URL of web resource to get</param>
        /// <returns>newly created web resource response</returns>
        private WebResourceResponse BuildCorsResponse(string url)
        {
            Stream stream = this.GetUrlContentStream(url);
            if (stream == null)
            {
                return null;
            }

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

            return new WebResourceResponse(
                "text/plain",
                "UTF-8",
                200,
                "OK",
                responseHeaders,
                stream);
        }

        /// <summary>
        /// Gets URL content stream, either from the network, or from the cache when already
        /// downloaded. When fetching from the network, the content is also stored in the cache
        /// folder.
        /// </summary>
        /// <param name="url">URL to fetch</param>
        /// <returns>
        /// content stream, or null when there was an error getting the content stream
        /// </returns>
        private Stream GetUrlContentStream(string url)
        {
            int hashCode = url.GetHashCode();

            string cacheFilename = Path.Combine(this.cacheFolder, hashCode.ToString() + ".bin");

            if (File.Exists(cacheFilename))
            {
                this.numCacheHit++;
                Debug.WriteLine($"CORS cache hits vs misses: {this.numCacheHit}/{this.numCacheMiss}");

                return new FileStream(cacheFilename, FileMode.Open);
            }

            try
            {
                var data = this.httpClient.GetByteArrayAsync(url).Result;

                File.WriteAllBytes(cacheFilename, data);

                this.numCacheMiss++;
                Debug.WriteLine($"CORS cache hits vs misses: {this.numCacheHit}/{this.numCacheMiss}");

                return new MemoryStream(data);
            }
            catch (Exception)
            {
                // ignore exceptions when fetching content (and use null stream)
            }

            return null;
        }

        /// <summary>
        /// Retrieves cache folder and ensures it exists
        /// </summary>
        /// <returns>file name of cache folder</returns>
        private static string GetCacheFolder()
        {
            string corsCacheFolder = Path.Combine(
                FileSystem.CacheDirectory,
                "cors-cache");

            if (!Directory.Exists(corsCacheFolder))
            {
                try
                {
                    Directory.CreateDirectory(corsCacheFolder);
                }
                catch (Exception)
                {
                    // ignore error
                }
            }

            return corsCacheFolder;
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

            if (disposing && this.httpClient != null)
            {
                this.httpClient.Dispose();
            }
        }
    }
}
