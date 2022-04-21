using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WhereToFly.WebApi.Logic
{
    /// <summary>
    /// Cache for favicon URLs for websites
    /// </summary>
    public class FaviconUrlCache
    {
        /// <summary>
        /// HTTP client to use to download index pages
        /// </summary>
        private readonly HttpClient client = new();

        /// <summary>
        /// Favicon URL cache; key is the base URI, value is the favicon URL
        /// </summary>
        private readonly Dictionary<string, string> urlCache = new();

        /// <summary>
        /// Creates a new favicon URL cache
        /// </summary>
        public FaviconUrlCache()
        {
            // pre-fill the cache with some sites that don't like to get their index page parsed
            this.urlCache["https://www.wetteronline.de/"] = "https://st.wetteronline.de/images/logo/woicon_180x180.png";
        }

        /// <summary>
        /// Retrieves a favicon URL for given URI
        /// </summary>
        /// <param name="uri">website URI to use</param>
        /// <returns>favicon URL</returns>
        public async Task<string> GetFaviconUrlAsync(Uri uri)
        {
            // download index page using hostname
            string baseUri = $"{uri.Scheme}://{uri.Host}/";

            if (this.urlCache.ContainsKey(baseUri))
            {
                return await Task.FromResult(this.urlCache[baseUri]);
            }

            var htmlDocument = await this.DownloadIndexPage(baseUri);

            string iconUrl = FindShortcutIconInDocument(baseUri, htmlDocument);
            if (iconUrl == null)
            {
                var faviconUri = new Uri(new Uri(baseUri), "favicon.ico");
                iconUrl = faviconUri.AbsoluteUri;
            }

            int pos = iconUrl.IndexOf(";jsessionid=");
            if (pos != -1)
            {
                iconUrl = iconUrl.Substring(0, pos);
            }

            this.urlCache[baseUri] = iconUrl;

            return iconUrl;
        }

        /// <summary>
        /// Downloads index page for base URL and returns a parsed HTML document
        /// </summary>
        /// <param name="baseUrl">base URL of index page to get</param>
        /// <returns>parsed HTML document</returns>
        private async Task<HtmlDocument> DownloadIndexPage(string baseUrl)
        {
            Stream textStream = await this.client.GetStreamAsync(baseUrl);

            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(textStream);

            return htmlDocument;
        }

        /// <summary>
        /// Finds a shortcut icon in the document by trying to find various link HTML tags.
        /// </summary>
        /// <param name="baseUri">base URI to use</param>
        /// <param name="htmlDocument">HTML document to check</param>
        /// <returns>shortcut icon, or null when none was found</returns>
        private static string FindShortcutIconInDocument(string baseUri, HtmlDocument htmlDocument)
        {
            string[] htmlQueriesList = new string[]
            {
                "//link[@rel='apple-touch-icon']",
                "//link[@rel='apple-touch-icon-precomposed']",
                "//link[@rel='shortcut icon']",
                "//link[@rel='icon']",
            };

            foreach (var htmlQuery in htmlQueriesList)
            {
                try
                {
                    var iconUrl = TryGetDocumentIcon(htmlDocument, baseUri, htmlQuery, "href");
                    if (iconUrl != null)
                    {
                        return iconUrl;
                    }
                }
                catch (Exception)
                {
                    // ignore exception
                }
            }

            return null;
        }

        /// <summary>
        /// Tries to get HTML document's icon with given HTML query
        /// </summary>
        /// <param name="htmlDocument">HTML document to query</param>
        /// <param name="baseUri">base URI of document</param>
        /// <param name="htmlQuery">HTML query to use</param>
        /// <param name="attributeName">attribute's name to get value for</param>
        /// <returns>found document icon URI, or null when not found</returns>
        private static string TryGetDocumentIcon(HtmlDocument htmlDocument, string baseUri, string htmlQuery, string attributeName)
        {
            var rootNode = htmlDocument.DocumentNode;

            var selectedNode = rootNode.SelectSingleNode(htmlQuery);
            if (selectedNode != null &&
                selectedNode.Attributes.Contains(attributeName))
            {
                var linkUri = selectedNode.Attributes[attributeName].Value;
                var completeUri = new Uri(new Uri(baseUri), linkUri);

                return completeUri.AbsoluteUri;
            }

            return null;
        }
    }
}
