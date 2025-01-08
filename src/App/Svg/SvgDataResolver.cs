using SkiaSharp.Extended.Svg;
using System.Text;

namespace WhereToFly.App.Svg
{
    /// <summary>
    /// Resolves loading SVG image data from various image sources
    /// </summary>
    internal static class SvgDataResolver
    {
        /// <summary>
        /// Cache for SKSvg objects
        /// </summary>
        private static readonly Dictionary<string, SKSvg> SvgCache = [];

        /// <summary>
        /// Loads SVG image from given image source
        /// </summary>
        /// <param name="source">image source to use for loading</param>
        /// <returns>loaded SVG image object</returns>
        /// <exception cref="Exception">thrown when loading has failed</exception>
        public static async Task<SKSvg?> LoadSvgImage(ImageSource? source)
        {
            return source switch
            {
                StreamImageSource streamSource => await LoadImageFromStream(streamSource),
                UriImageSource uriImageSource => LoadImageFromUri(uriImageSource.Uri),
                FileImageSource fileImageSource => LoadImageFromFile(fileImageSource.File),
                _ => null,
            };
        }

        /// <summary>
        /// Loads SVG image from stream image source
        /// </summary>
        /// <param name="streamSource">stream image source</param>
        /// <returns>loaded SVG image object, or null when loading was not successful</returns>
        private static async Task<SKSvg> LoadImageFromStream(StreamImageSource streamSource)
        {
            if (streamSource.Stream == null)
            {
                throw new InvalidOperationException("StreamImageSource stream is null");
            }

            var stream = await streamSource.Stream(CancellationToken.None);

            if (stream == null || stream == Stream.Null)
            {
                throw new InvalidOperationException("StreamImageSource stream is null or Stream.Null");
            }

            string? cacheKey = null;
            if (stream is FileStream fileStream)
            {
                cacheKey = "FileStream:" + fileStream.Name;
                if (SvgCache.TryGetValue(cacheKey, out var value))
                {
                    return value;
                }
            }

            var svg = new SKSvg();
            svg.Load(stream);

            if (cacheKey != null)
            {
                SvgCache[cacheKey] = svg;
            }

            return svg;
        }

        /// <summary>
        /// Loads image from an URI
        /// </summary>
        /// <param name="uri">resource or data URI</param>
        /// <returns>loaded SVG image object, or null when loading was not successful</returns>
        private static SKSvg LoadImageFromUri(Uri uri)
        {
            if (uri.Scheme == "data")
            {
                return LoadImageFromDataUri(uri);
            }

            throw new NotSupportedException("URI not supported: " + uri.OriginalString);
        }

        /// <summary>
        /// Loads image from data URI
        /// </summary>
        /// <param name="uri">data URI</param>
        /// <returns>loaded SVG image object, or null when loading was not successful</returns>
        private static SKSvg LoadImageFromDataUri(Uri uri)
        {
            string text = uri.OriginalString;

            string cacheKey = "DataUri:" + text;
            if (SvgCache.TryGetValue(cacheKey, out var value))
            {
                return value;
            }

            string? svgText = null;
            if (text.StartsWith(SvgConstants.DataUriPlainPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                svgText = text.Substring(SvgConstants.DataUriPlainPrefix.Length);
            }
            else if (text.StartsWith(SvgConstants.DataUriBase64Prefix, StringComparison.InvariantCultureIgnoreCase))
            {
                svgText = text.Substring(SvgConstants.DataUriBase64Prefix.Length);

                byte[] base64EncodedBytes = Convert.FromBase64String(svgText);
                svgText = Encoding.UTF8.GetString(base64EncodedBytes);
            }

            if (svgText == null)
            {
                throw new FormatException("Invalid SVG image text format: " + uri.OriginalString);
            }

            var svg = new SKSvg();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgText));
            svg.Load(stream);

            SvgCache[cacheKey] = svg;

            return svg;
        }

        /// <summary>
        /// Loads image from filename
        /// </summary>
        /// <param name="filename">image filename</param>
        /// <returns>loaded SVG image object, or null when loading was not successful</returns>
        private static SKSvg LoadImageFromFile(string filename)
        {
            string cacheKey = "Filename:" + filename;
            if (SvgCache.TryGetValue(cacheKey, out var value))
            {
                return value;
            }

            var svg = new SKSvg();
            svg.Load(filename);

            SvgCache[cacheKey] = svg;

            return svg;
        }
    }
}
