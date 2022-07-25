using SkiaSharp.Extended.Svg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

#nullable enable

namespace WhereToFly.App.Core.Controls
{
    /// <summary>
    /// Resolves loading SVG image data from various image sources
    /// </summary>
    internal static class SvgDataResolver
    {
        /// <summary>
        /// Cache for SKSvg objects
        /// </summary>
        private static Dictionary<string, SKSvg> svgCache = new();

        /// <summary>
        /// Loads SVG image from given image source
        /// </summary>
        /// <param name="source">image source to use for loading</param>
        /// <returns>loaded SVG image object</returns>
        /// <exception cref="Exception">thrown when loading has failed</exception>
        public static async Task<SKSvg?> LoadSvgImage(ImageSource source)
        {
            switch (source)
            {
                case StreamImageSource streamSource:
                    return await LoadImageFromStream(streamSource);
                case UriImageSource uriImageSource:
                    return LoadImageFromUri(uriImageSource.Uri);
                case FileImageSource fileImageSource:
                    return LoadImageFromFile(fileImageSource.File);
            }

            return null;
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
                if (svgCache.ContainsKey(cacheKey))
                {
                    return svgCache[cacheKey];
                }
            }

            var svg = new SKSvg();
            svg.Load(stream);

            if (cacheKey != null)
            {
                svgCache[cacheKey] = svg;
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
            if (svgCache.ContainsKey(cacheKey))
            {
                return svgCache[cacheKey];
            }

            string? svgText = null;
            if (text.StartsWith(SvgConstants.DataUriPlainPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                svgText = text.Substring(SvgConstants.DataUriPlainPrefix.Length);
            }
            else if (text.StartsWith(SvgConstants.DataUriBase64Prefix, StringComparison.InvariantCultureIgnoreCase))
            {
                svgText = text.Substring(SvgConstants.DataUriBase64Prefix.Length);

                var base64EncodedBytes = Convert.FromBase64String(svgText);
                svgText = Encoding.UTF8.GetString(base64EncodedBytes);
            }

            if (svgText == null)
            {
                throw new FormatException("Invalid SVG image text format: " + uri.OriginalString);
            }

            var svg = new SKSvg();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgText));
            svg.Load(stream);

            svgCache[cacheKey] = svg;

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
            if (svgCache.ContainsKey(cacheKey))
            {
                return svgCache[cacheKey];
            }

            var svg = new SKSvg();
            svg.Load(filename);

            svgCache[cacheKey] = svg;

            return svg;
        }
    }
}
