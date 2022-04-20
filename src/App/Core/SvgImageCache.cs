using FFImageLoading.Svg.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WhereToFly.Geo.Model;
using Xamarin.Forms;

#pragma warning disable S4136 // All 'GetImageSource' method overloads should be adjacent.

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
        private readonly Dictionary<string, string> allImages = new();

        /// <summary>
        /// Returns an image source for SvgImage that loads an image based on the location's type.
        /// </summary>
        /// <param name="location">location to use</param>
        /// <returns>image source</returns>
        public static ImageSource GetImageSource(Location location)
        {
            if (location == null)
            {
                return null;
            }

            return GetImageSource(
                SvgImagePathFromLocationType(location.Type));
        }

        /// <summary>
        /// Returns an image source for SvgImage that loads an image based on the track.
        /// </summary>
        /// <param name="track">track to use</param>
        /// <returns>image source</returns>
        public static ImageSource GetImageSource(Track track)
        {
            string svgImagePath = track.IsFlightTrack ? "weblib/images/paragliding.svg" : "icons/map-marker-distance.svg";

            return GetImageSource(svgImagePath);
        }

        /// <summary>
        /// Returns an image source for SvgImage that loads an image based on the given layer.
        /// </summary>
        /// <param name="layer">layer to use</param>
        /// <returns>image source</returns>
        public static ImageSource GetImageSource(Layer layer)
        {
            string svgImagePath = ImagePathFromLayerType(layer.LayerType);

            return GetImageSource(svgImagePath);
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
        /// <returns>image source</returns>
        public static ImageSource GetLayerVisibilityImageSource(Layer layer)
        {
            string svgImagePath =
                layer.IsVisible ? "icons/eye.svg" : "icons/eye-off-outline.svg";

            return GetImageSource(svgImagePath);
        }

        /// <summary>
        /// Returns an image source for SvgImage that loads an image from given SVG image path and
        /// name.
        /// </summary>
        /// <param name="svgImageName">relative path to the SVG image file</param>
        /// <param name="noReplaceStringMap">when true, no replace string map is set</param>
        /// <returns>image source</returns>
        public static ImageSource GetImageSource(string svgImageName, bool noReplaceStringMap = false)
        {
            var cache = DependencyService.Get<SvgImageCache>();
            Debug.Assert(cache != null, "cache object must exist");

            string svgText = cache.GetSvgImage(svgImageName);

            Dictionary<string, string> replaceStringMap = null;
            if (!noReplaceStringMap &&
                Application.Current?.Resources != null &&
                Application.Current.Resources.ContainsKey("SvgImageFillDark"))
            {
                replaceStringMap = Application.Current.Resources["SvgImageFillDark"] as Dictionary<string, string>;
            }

            return SvgImageSource.FromSvgString(svgText, replaceStringMap: replaceStringMap);
        }

        /// <summary>
        /// Returns an SVG image xml text from an image path
        /// </summary>
        /// <param name="imagePath">image path</param>
        /// <returns>SVG image xml text, or </returns>
        public string GetSvgImage(string imagePath)
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
                    return "weblib/images/mountain-15.svg";
                case LocationType.Pass:
                    return "weblib/images/mountain-pass.svg";
                case LocationType.Lake:
                    return "weblib/images/water-15.svg";
                case LocationType.Bridge:
                    return "weblib/images/bridge.svg";
                case LocationType.Viewpoint:
                    return "weblib/images/attraction-15.svg";
                case LocationType.AlpineHut:
                    return "weblib/images/home-15.svg";
                case LocationType.Restaurant:
                    return "weblib/images/restaurant-15.svg";
                case LocationType.Church:
                    return "weblib/images/church.svg";
                case LocationType.Castle:
                    return "weblib/images/castle.svg";
                case LocationType.Cave:
                    return "weblib/images/cave.svg";
                case LocationType.Information:
                    return "weblib/images/information-outline.svg";
                case LocationType.PublicTransportBus:
                    return "weblib/images/bus.svg";
                case LocationType.PublicTransportTrain:
                    return "weblib/images/train.svg";
                case LocationType.Parking:
                    return "weblib/images/parking.svg";
                case LocationType.Webcam:
                    return "weblib/images/camera-outline.svg";
                case LocationType.CableCar:
                    return "weblib/images/aerialway-15.svg";
                case LocationType.FlyingTakeoff:
                    return "weblib/images/paragliding.svg";
                case LocationType.FlyingLandingPlace:
                    return "weblib/images/paragliding.svg";
                case LocationType.FlyingWinchTowing:
                    return "weblib/images/paragliding.svg";
                case LocationType.LiveWaypoint:
                    return "weblib/images/autorenew.svg";
                case LocationType.Thermal:
                    return "weblib/images/cloud-upload-outline-modified.svg";
                case LocationType.MeteoStation:
                    return "weblib/images/weather-partly-cloudy.svg";
                default:
                    return "weblib/images/map-marker.svg";
            }
        }
    }
}
