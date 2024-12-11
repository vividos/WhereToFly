using WhereToFly.App.Models;
using WhereToFly.App.Resources;

namespace WhereToFly.App.Logic
{
    /// <summary>
    /// Image cache for weather images
    /// </summary>
    public static class WeatherImageCache
    {
        /// <summary>
        /// Returns an image from image cache
        /// </summary>
        /// <param name="iconDescription">weather icon description to load image for</param>
        /// <returns>image source, or null when no image was found or could be loaded</returns>
        public static async Task<ImageSource?> GetImageAsync(WeatherIconDescription iconDescription)
        {
            switch (iconDescription.Type)
            {
                case WeatherIconDescription.IconType.IconLink:
                    if (iconDescription.WebLink.StartsWith("https://www.austrocontrol.at"))
                    {
                        return ImageSource.FromStream(
                            async (cancellationToken) =>
                                await Assets.Get("alptherm-favicon.png"));
                    }

                    string faviconLink = await GetFaviconFromLinkAsync(iconDescription.WebLink);

                    if (!string.IsNullOrEmpty(faviconLink))
                    {
                        return new UriImageSource { Uri = new Uri(faviconLink) };
                    }

                    break;

                case WeatherIconDescription.IconType.IconApp:
                    return ImageSource.FromStream(
                        (cancellationToken) =>
                        {
                            var appManager = DependencyService.Get<IAppManager>();
                            byte[]? appIconData = appManager.GetAppIcon(iconDescription.WebLink);

                            return Task.FromResult<Stream?>(
                                appIconData != null
                                ? new MemoryStream(appIconData)
                                : null);
                        });

                case WeatherIconDescription.IconType.IconPlaceholder:
                    return ImageSource.FromFile("border_none_variant.png");

                default:
                    break;
            }

            return null;
        }

        /// <summary>
        /// Returns a favicon link from given web link
        /// </summary>
        /// <param name="webLink">web link</param>
        /// <returns>link with hostname and favicon.ico prefixed</returns>
        private static async Task<string> GetFaviconFromLinkAsync(string webLink)
        {
            if (webLink.StartsWith("https://www.austrocontrol.at"))
            {
                return "https://www.austrocontrol.at/favicon.ico";
            }

            int pos = webLink.IndexOf(";jsessionid=");
            if (pos != -1)
            {
                webLink = webLink.Substring(0, pos);
            }

            var dataService = DependencyService.Get<IDataService>();

            return await dataService.GetFaviconUrlAsync(webLink);
        }
    }
}
