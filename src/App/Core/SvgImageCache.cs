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
            string svgImagePath = track.IsFlightTrack
                ? "weblib/images/paragliding.svg"
                : "icons/map-marker-distance.svg";

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
            return layerType switch
            {
                LayerType.LocationLayer => "icons/format-list-bulleted.svg",
                LayerType.TrackLayer => "icons/map-marker-distance.svg",
                _ => "icons/layers-outline.svg",
            };
        }

        /// <summary>
        /// Returns an image source for the visibility of the given layer.
        /// </summary>
        /// <param name="layer">layer to use</param>
        /// <returns>image source</returns>
        public static ImageSource GetLayerVisibilityImageSource(Layer layer)
        {
            string svgImagePath = layer.IsVisible
                ? "icons/eye.svg"
                : "icons/eye-off-outline.svg";

            return GetImageSource(svgImagePath);
        }

        /// <summary>
        /// Returns an image source for SvgImage that loads an image from given SVG image path and
        /// name.
        /// </summary>
        /// <param name="svgImageName">relative path to the SVG image file</param>
        /// <returns>image source</returns>
        public static ImageSource GetImageSource(string svgImageName)
        {
            var cache = DependencyService.Get<SvgImageCache>();
            Debug.Assert(cache != null, "cache object must exist");

            string svgText = cache.GetSvgImage(svgImageName);

            return ImageSource.FromUri(new Uri(Controls.SvgConstants.DataUriPlainPrefix + svgText));
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
            return locationType switch
            {
                LocationType.Summit => "weblib/images/mountain-15.svg",
                LocationType.Pass => "weblib/images/mountain-pass.svg",
                LocationType.Lake => "weblib/images/water-15.svg",
                LocationType.Bridge => "weblib/images/bridge.svg",
                LocationType.Viewpoint => "weblib/images/attraction-15.svg",
                LocationType.AlpineHut => "weblib/images/home-15.svg",
                LocationType.Restaurant => "weblib/images/restaurant-15.svg",
                LocationType.Church => "weblib/images/church.svg",
                LocationType.Castle => "weblib/images/castle.svg",
                LocationType.Cave => "weblib/images/cave.svg",
                LocationType.Information => "weblib/images/information-outline.svg",
                LocationType.PublicTransportBus => "weblib/images/bus.svg",
                LocationType.PublicTransportTrain => "weblib/images/train.svg",
                LocationType.Parking => "weblib/images/parking.svg",
                LocationType.Webcam => "weblib/images/camera-outline.svg",
                LocationType.CableCar => "weblib/images/aerialway-15.svg",
                LocationType.FlyingTakeoff => "weblib/images/paragliding.svg",
                LocationType.FlyingLandingPlace => "weblib/images/paragliding.svg",
                LocationType.FlyingWinchTowing => "weblib/images/paragliding.svg",
                LocationType.LiveWaypoint => "weblib/images/autorenew.svg",
                LocationType.Thermal => "weblib/images/cloud-upload-outline-modified.svg",
                LocationType.MeteoStation => "weblib/images/weather-partly-cloudy.svg",
                _ => "weblib/images/map-marker.svg",
            };
        }
    }
}
