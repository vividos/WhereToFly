using System;
using System.Text;

namespace WhereToFly.App.UnitTest.Controls
{
    /// <summary>
    /// SVG test images
    /// </summary>
    public static class SvgTestImages
    {
        /// <summary>
        /// Test SVG image as text; see also shapes.svg
        /// </summary>
        public const string TestSvgImageText =
            @"<svg width=""64"" height=""64"" xmlns=""http://www.w3.org/2000/svg"">
  <rect x=""10"" y=""15"" width=""20"" height=""30"" style=""fill:rgb(0,255,0)"" />
  <circle cx=""45"" cy=""40"" r=""15"" stroke=""black"" stroke-width=""3"" fill=""red"" />
  <path fill=""#ff0"" stroke-width=""2"" stroke=""green"" d=""M60 60 H30 L15 30 Z"" />
</svg>";

        /// <summary>
        /// Resource path to colibri.svg in the Sample assembly
        /// </summary>
        public const string ResourcePathColibriSvg =
            "WhereToFly.App.UnitTest.Assets.svg.colibri.svg";

        /// <summary>
        /// Encodes SVG image text as Base64
        /// </summary>
        /// <param name="svgImageText">image text</param>
        /// <returns>Base64 encoded text</returns>
        public static string EncodeBase64(string svgImageText)
        {
            var utf8bytes = Encoding.UTF8.GetBytes(svgImageText);
            return Convert.ToBase64String(utf8bytes);
        }
    }
}
