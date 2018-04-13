using System.Collections.Generic;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

[assembly: Dependency(typeof(WhereToFly.Core.SvgImageCache))]

namespace WhereToFly.Core
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
        /// Returns an SVG image xml text based on the location type
        /// </summary>
        /// <param name="type">location type</param>
        /// <param name="fill">when not null, an alternative fill color for SVG path elements</param>
        /// <returns>SVG image xml text, or </returns>
        public string GetSvgImageByLocationType(LocationType type, string fill = null)
        {
            string imagePath = SvgImagePathFromLocationType(type);

            return this.GetSvgImage(imagePath, fill);
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
            string svgText = platform.LoadAssetText(imagePath);

            if (svgText != null && fill != null)
            {
                svgText = this.ReplaceSvgPathFillValue(svgText, fill);
            }

            this.allImages[imagePath] = svgText;

            return svgText;
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
