using Android.Webkit;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// CORS website cache
    /// </summary>
    internal class CorsWebsiteCache
    {
        /// <summary>
        /// HTTP client to use to download content when intercepting content
        /// </summary>
        private readonly HttpClient httpClient = new();

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
        /// since some web sites don't send CORS headers.
        /// </summary>
        public HashSet<string> CorsWebsiteHosts { get; set; } = new();

        /// <summary>
        /// Creates new CORS website cache
        /// </summary>
        public CorsWebsiteCache()
        {
            this.cacheFolder = GetCacheFolder();
        }

        /// <summary>
        /// Checks if the web resource request should be intercepted by the CORS cache.
        /// </summary>
        /// <param name="request">web resource request</param>
        /// <returns>
        /// web resource response, or null when the request was not processed
        /// </returns>
        public WebResourceResponse? ShouldInterceptRequest(IWebResourceRequest? request)
        {
            string? host = request?.Url?.Host?.ToLowerInvariant();

            if (request?.Url != null &&
                host != null &&
                this.CorsWebsiteHosts != null &&
                this.CorsWebsiteHosts.Any(host.Contains))
            {
                var response = this.BuildCorsResponse(request.Url.ToString());
                if (response != null)
                {
                    return response;
                }
            }

            return null;
        }

        /// <summary>
        /// Builds a new web resource response that circumvents the CORS headers sent while
        /// fetching the URL content using HttpClient.
        /// </summary>
        /// <param name="url">URL of web resource to get</param>
        /// <returns>newly created web resource response</returns>
        private WebResourceResponse? BuildCorsResponse(string? url)
        {
            if (url == null)
            {
                return null;
            }

            Stream? stream = this.GetUrlContentStream(url);
            if (stream == null)
            {
                return null;
            }

            string date = DateTime.Now.ToUniversalTime().ToString("r");
            string domainName = "https://appassets.androidplatform.net";

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
        private Stream? GetUrlContentStream(string url)
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
                byte[] data = this.httpClient.GetByteArrayAsync(url).Result;

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
    }
}
