using System;
using System.Collections.Generic;
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
        /// Lock for access to image cache dictionary
        /// </summary>
        private readonly object imageCacheLock = new object();

        /// <summary>
        /// Image cache dictionary
        /// </summary>
        private readonly Dictionary<string, ImageSource> imageCache = new Dictionary<string, ImageSource>();

        /// <summary>
        /// Returns an image from image cache
        /// </summary>
        /// <param name="iconDescription">weather icon description to load image for</param>
        /// <returns>image source, or null when no image was found or could be loaded</returns>
        public async Task<ImageSource> GetImageAsync(WeatherIconDescription iconDescription)
        {
            string imageId = GetImageIdentifier(iconDescription);

            bool containsImage;
            lock (this.imageCacheLock)
            {
                containsImage = this.imageCache.ContainsKey(imageId);
            }

            if (!containsImage)
            {
                await this.LoadImageAsync(imageId, iconDescription);
            }

            return this.imageCache[imageId] ?? null;
        }

        /// <summary>
        /// Gets image identifier for given weather icon description
        /// </summary>
        /// <param name="iconDescription">weather icon description</param>
        /// <returns>image identifier string</returns>
        private static string GetImageIdentifier(WeatherIconDescription iconDescription)
        {
            string identifier = iconDescription.Type + "|";

            switch (iconDescription.Type)
            {
                case WeatherIconDescription.IconType.IconLink:
                    identifier += GetFaviconFromLink(iconDescription.WebLink);
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
        private static string GetFaviconFromLink(string webLink)
        {
            var url = new Uri(webLink);

            return $"http://www.google.com/s2/favicons?domain={url.Host}";
        }

        /// <summary>
        /// Loads image with given image ID and weather icon description
        /// </summary>
        /// <param name="imageId">image ID to load</param>
        /// <param name="iconDescription">icon description for image to load</param>
        /// <returns>task to wait on</returns>
        private Task LoadImageAsync(string imageId, WeatherIconDescription iconDescription)
        {
            ImageSource imageSource = null;

            switch (iconDescription.Type)
            {
                case WeatherIconDescription.IconType.IconLink:
                    string faviconLink = GetFaviconFromLink(iconDescription.WebLink);
                    imageSource = ImageSource.FromUri(new Uri(faviconLink));
                    break;

                case WeatherIconDescription.IconType.IconApp:
                    var appManager = DependencyService.Get<IAppManager>();
                    imageSource = appManager.GetAppIcon(iconDescription.WebLink);
                    break;

                case WeatherIconDescription.IconType.IconPlaceholder:
                    imageSource = ImageSource.FromFile("border_none_variant.xml");
                    break;

                default:
                    break;
            }

            if (imageSource != null)
            {
                lock (this.imageCacheLock)
                {
                    this.imageCache[imageId] = imageSource;
                }
            }

            return Task.CompletedTask;
        }
    }
}
