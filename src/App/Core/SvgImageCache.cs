using FFImageLoading.Svg.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WhereToFly.App.Geo;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

[assembly: Dependency(typeof(WhereToFly.App.Core.SvgImageCache))]

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Cache for SVG images
    /// </summary>
    public class SvgImageCache
    {
        /// <summary>
        /// All SVG images in the cache
        /// </summary>
        private readonly Dictionary<string, string> allImages = new Dictionary<string, string>();

        /// <summary>
        /// Returns an image source for SvgImage that loads an image based on the location's type.
        /// </summary>
        /// <param name="location">location to use</param>
        /// <param name="fill">when not null, an alternative fill color for SVG path elements</param>
        /// <returns>image source</returns>
        public static ImageSource GetImageSource(Location location, string fill = null)
        {
            if (location == null)
            {
                return null;
            }

            return GetImageSource(
                SvgImagePathFromLocationType(location.Type),
                fill);
        }

        /// <summary>
        /// Returns an image source for SvgImage that loads an image based on the track.
        /// </summary>
        /// <param name="track">track to use</param>
        /// <param name="fill">when not null, an alternative fill color for SVG path elements</param>
        /// <returns>image source</returns>
        public static ImageSource GetImageSource(Track track, string fill = null)
        {
            string svgImagePath = track.IsFlightTrack ? "map/images/paragliding.svg" : "icons/map-marker-distance.svg";

            return GetImageSource(svgImagePath, fill);
        }

        /// <summary>
        /// Returns an image source for SvgImage that loads an image based on the given layer.
        /// </summary>
        /// <param name="layer">layer to use</param>
        /// <param name="fill">when not null, an alternative fill color for SVG path elements</param>
        /// <returns>image source</returns>
        public static ImageSource GetImageSource(Layer layer, string fill = null)
        {
            string svgImagePath = ImagePathFromLayerType(layer.LayerType);

            return GetImageSource(svgImagePath, fill);
        }

        /// <summary>
        /// Returns SVG image path from layer type
        /// </summary>
        /// <param name="layerType">layer type</param>
        /// <returns>SVG image path</returns>
        private static string ImagePathFromLayerType(LayerType layerType)
        {
            switch (layerType)
            {
                case LayerType.LocationLayer:
                    return "icons/format-list-bulleted.svg";
                case LayerType.TrackLayer:
                    return "icons/map-marker-distance.svg";
                default:
                    return "icons/layers-outline.svg";
            }
        }

        /// <summary>
        /// Returns an image source for the visibility of the given layer.
        /// </summary>
        /// <param name="layer">layer to use</param>
        /// <param name="fill">when not null, an alternative fill color for SVG path elements</param>
        /// <returns>image source</returns>
        public static ImageSource GetLayerVisibilityImageSource(Layer layer, string fill = null)
        {
            string svgImagePath =
                layer.IsVisible ? "icons/eye.svg" : "icons/eye-off-outline.svg";

            return GetImageSource(svgImagePath, fill);
        }

        /// <summary>
        /// Returns an image source for SvgImage that loads an image from given SVG image path and
        /// name.
        /// </summary>
        /// <param name="svgImageName">relative path to the SVG image file</param>
        /// <param name="fill">when not null, an alternative fill color for SVG path elements</param>
        /// <returns>image source</returns>
        public static ImageSource GetImageSource(string svgImageName, string fill = null)
        {
            var cache = DependencyService.Get<SvgImageCache>();
            Debug.Assert(cache != null, "cache object must exist");

            string svgText = cache.GetSvgImage(svgImageName, fill);

            return SvgImageSource.FromSvgString(svgText);
        }

        /// <summary>
        /// Returns an SVG image xml text from an image path
        /// </summary>
        /// <param name="imagePath">image path</param>
        /// <param name="fill">when not null, an alternative fill color for SVG path elements</param>
        /// <returns>SVG image xml text, or </returns>
        public string GetSvgImage(string imagePath, string fill = null)
        {
            if (this.allImages.ContainsKey(imagePath))
            {
                return this.allImages[imagePath];
            }

            var platform = DependencyService.Get<IPlatform>();

            string svgText = null;
            try
            {
                svgText = platform.LoadAssetText(imagePath);
            }
            catch (Exception)
            {
                // ignore load errors
            }

            if (svgText != null && fill != null)
            {
                svgText = this.ReplaceSvgPathFillValue(svgText, fill);
            }

            this.AddImage(imagePath, svgText);

            return svgText;
        }

        /// <summary>
        /// Adds SVG image text to image cache
        /// </summary>
        /// <param name="imagePath">path of image to add</param>
        /// <param name="svgText">SVG image text</param>
        internal void AddImage(string imagePath, string svgText)
        {
            this.allImages[imagePath] = svgText;
        }

        /// <summary>
        /// Returns an SVG image path from given location type
        /// </summary>
        /// <param name="locationType">location type</param>
        /// <returns>SVG image path</returns>
        private static string SvgImagePathFromLocationType(LocationType locationType)
        {
            switch (locationType)
            {
                case LocationType.Summit:
                    return "map/images/mountain-15.svg";
                case LocationType.Lake:
                    return "map/images/water-15.svg";
                case LocationType.Bridge:
                    return "map/images/bridge.svg";
                case LocationType.Viewpoint:
                    return "map/images/attraction-15.svg";
                case LocationType.AlpineHut:
                    return "map/images/home-15.svg";
                case LocationType.Restaurant:
                    return "map/images/restaurant-15.svg";
                case LocationType.Church:
                    return "map/images/church.svg";
                case LocationType.Castle:
                    return "map/images/castle.svg";
                case LocationType.Information:
                    return "map/images/information-outline.svg";
                case LocationType.PublicTransportBus:
                    return "map/images/bus.svg";
                case LocationType.PublicTransportTrain:
                    return "map/images/train.svg";
                case LocationType.Parking:
                    return "map/images/parking.svg";
                case LocationType.CableCar:
                    return "map/images/aerialway-15.svg";
                case LocationType.FlyingTakeoff:
                    return "map/images/paragliding.svg";
                case LocationType.FlyingLandingPlace:
                    return "map/images/paragliding.svg";
                case LocationType.FlyingWinchTowing:
                    return "map/images/paragliding.svg";
                default:
                    return "map/images/map-marker.svg";
            }
        }

        /// <summary>
        /// Replaces path fill attributes in an SVG image text, e.g. to re-colorize the image.
        /// </summary>
        /// <param name="svgText">SVG image text</param>
        /// <param name="fillValue">fill value to replace, e.g. #000000</param>
        /// <returns>new SVG image text</returns>
        private string ReplaceSvgPathFillValue(string svgText, string fillValue)
        {
            int pos = 0;
            while ((pos = svgText.IndexOf("<path fill=", pos)) != -1)
            {
                char openingQuote = svgText[pos + 11];
                int endPos = svgText.IndexOf(openingQuote, pos + 11 + 1);

                if (endPos != -1)
                {
                    svgText = svgText.Substring(0, pos + 11) + openingQuote + fillValue + svgText.Substring(endPos);
                }

                pos += 11;
            }

            return svgText;
        }
    }
}
