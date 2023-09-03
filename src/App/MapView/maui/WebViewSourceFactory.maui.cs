using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Web view source factory implementation for MAUI
    /// </summary>
    internal partial class WebViewSourceFactory
    {
        /// <summary>
        /// Loads text of MauiAsset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>text content of asset</returns>
        private static async Task<string> LoadAssetText(string assetFilename)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(assetFilename);

            if (stream == null)
            {
                return null;
            }

            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Returns HTML web view source for MapView
        /// </summary>
        /// <returns>web view source</returns>
        public async Task<WebViewSource> PlatformGetMapViewSource()
        {
            return new HtmlWebViewSource
            {
                Html = await LoadAssetText("weblib/mapView.html"),
            };
        }

        /// <summary>
        /// Returns HTML web view source for HeightProfileView
        /// </summary>
        /// <returns>web view source</returns>
        public async Task<WebViewSource> PlatformGetHeightProfileViewSource()
        {
            return new HtmlWebViewSource
            {
                Html = await LoadAssetText("weblib/heightProfileView.html"),
            };
        }
    }
}
