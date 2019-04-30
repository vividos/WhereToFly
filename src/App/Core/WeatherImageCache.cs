using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WhereToFly.App.Model;
using Xamarin.Forms;

[assembly: Dependency(typeof(WhereToFly.App.Core.WeatherImageCache))]

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Image cache for weather images
    /// </summary>
    public class WeatherImageCache
    {
        /// <summary>
        /// Entry in the image cache
        /// </summary>
        internal class ImageCacheEntry
        {
            /// <summary>
            /// Image source for displaying image
            /// </summary>
            [JsonIgnore]
            public Lazy<ImageSource> Source { get; private set; }

            /// <summary>
            /// Image data bytes
            /// </summary>
            public byte[] ImageData { get; set; }

            /// <summary>
            /// Creates a new image cache entry by using given factory function
            /// </summary>
            /// <param name="imageSourceFactory">factory function</param>
            public ImageCacheEntry(Func<ImageSource> imageSourceFactory)
            {
                this.ImageData = new byte[0];
                this.Source = new Lazy<ImageSource>(imageSourceFactory);
            }

            /// <summary>
            /// Creates a new image cache entry by using image data bytes
            /// </summary>
            /// <param name="imageData">image data bytes</param>
            public ImageCacheEntry(byte[] imageData)
            {
                Debug.Assert(imageData != null, "imageData must not be null");

                this.ImageData = imageData;
                this.Source = new Lazy<ImageSource>(this.CreateImageSource);
            }

            /// <summary>
            /// Creates a new image cache entry object that is used when deserializing; only sets
            /// the Source attribute, since the Image attribute is deserialized.
            /// </summary>
            [JsonConstructor]
            public ImageCacheEntry()
            {
                this.Source = new Lazy<ImageSource>(this.CreateImageSource);
            }

            /// <summary>
            /// Creates an image resource from stored image data bytes
            /// </summary>
            /// <returns>created image source</returns>
            private ImageSource CreateImageSource()
            {
                return ImageSource.FromStream(() => new MemoryStream(this.ImageData));
            }
        }

        /// <summary>
        /// Lock for access to image cache dictionary
        /// </summary>
        private readonly object imageCacheLock = new object();

        /// <summary>
        /// Image cache dictionary
        /// </summary>
        private readonly Dictionary<string, ImageCacheEntry> imageCache = new Dictionary<string, ImageCacheEntry>();

        /// <summary>
        /// HTTP client to download images from the internet
        /// </summary>
        private readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Loads weather image cache from given filename; file must exist
        /// </summary>
        /// <param name="cacheFilename">cache filename</param>
        public void LoadCache(string cacheFilename)
        {
            string json = File.ReadAllText(cacheFilename);
            var localCache = JsonConvert.DeserializeObject<Dictionary<string, ImageCacheEntry>>(json);

            lock (this.imageCacheLock)
            {
                foreach (var item in localCache)
                {
                    if (!this.imageCache.ContainsKey(item.Key) &&
                        item.Value.ImageData != null &&
                        item.Value.ImageData.Length > 0)
                    {
                        this.imageCache.Add(item.Key, item.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Stores weather image cache to given filename
        /// </summary>
        /// <param name="cacheFilename">cache filename</param>
        public void StoreCache(string cacheFilename)
        {
            string json;
            lock (this.imageCacheLock)
            {
                json = JsonConvert.SerializeObject(this.imageCache);
            }

            if (!string.IsNullOrEmpty(json))
            {
                File.WriteAllText(cacheFilename, json);
            }
        }

        /// <summary>
        /// Returns an image from image cache
        /// </summary>
        /// <param name="iconDescription">weather icon description to load image for</param>
        /// <returns>image source, or null when no image was found or could be loaded</returns>
        public async Task<ImageSource> GetImageAsync(WeatherIconDescription iconDescription)
        {
            string imageId = await GetImageIdentifierAsync(iconDescription);

            bool containsImage;
            lock (this.imageCacheLock)
            {
                containsImage = this.imageCache.ContainsKey(imageId);
            }

            if (!containsImage)
            {
                await this.LoadImageAsync(imageId, iconDescription);
            }

            return this.imageCache[imageId].Source.Value ?? null;
        }

        /// <summary>
        /// Gets image identifier for given weather icon description
        /// </summary>
        /// <param name="iconDescription">weather icon description</param>
        /// <returns>image identifier string</returns>
        private static async Task<string> GetImageIdentifierAsync(WeatherIconDescription iconDescription)
        {
            string identifier = iconDescription.Type + "|";

            switch (iconDescription.Type)
            {
                case WeatherIconDescription.IconType.IconLink:
                    identifier += await GetFaviconFromLinkAsync(iconDescription.WebLink);
                    break;

                case WeatherIconDescription.IconType.IconApp:
                    identifier += iconDescription.WebLink;
                    break;

                case WeatherIconDescription.IconType.IconPlaceholder:
                    identifier += "placeholder";
                    break;

                default:
                    break;
            }

            return identifier;
        }

        /// <summary>
        /// Returns a favicon link from given web link
        /// </summary>
        /// <param name="webLink">web link</param>
        /// <returns>link with hostname and favicon.ico prefixed</returns>
        private static async Task<string> GetFaviconFromLinkAsync(string webLink)
        {
            int pos = webLink.IndexOf(";jsessionid=");
            if (pos != -1)
            {
                webLink = webLink.Substring(0, pos);
            }

            var dataService = DependencyService.Get<IDataService>();

            return await dataService.GetFaviconUrlAsync(webLink);
        }

        /// <summary>
        /// Loads image with given image ID and weather icon description
        /// </summary>
        /// <param name="imageId">image ID to load</param>
        /// <param name="iconDescription">icon description for image to load</param>
        /// <returns>task to wait on</returns>
        private async Task LoadImageAsync(string imageId, WeatherIconDescription iconDescription)
        {
            ImageCacheEntry entry = null;

            switch (iconDescription.Type)
            {
                case WeatherIconDescription.IconType.IconLink:
                    string faviconLink = await GetFaviconFromLinkAsync(iconDescription.WebLink);

                    byte[] data = await this.client.GetByteArrayAsync(faviconLink);

                    entry = new ImageCacheEntry(data);
                    break;

                case WeatherIconDescription.IconType.IconApp:
                    var appManager = DependencyService.Get<IAppManager>();
                    entry = new ImageCacheEntry(appManager.GetAppIcon(iconDescription.WebLink));
                    break;

                case WeatherIconDescription.IconType.IconPlaceholder:
                    string imagePath = Converter.ImagePathConverter.GetDeviceDependentImage("border_none_variant");
                    entry = new ImageCacheEntry(() => ImageSource.FromFile(imagePath));
                    break;

                default:
                    break;
            }

            if (entry != null)
            {
                lock (this.imageCacheLock)
                {
                    this.imageCache[imageId] = entry;
                }
            }
        }
    }
}
