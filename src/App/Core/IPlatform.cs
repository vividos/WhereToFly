using System.IO;
using Xamarin.Forms;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Platform abstraction interface
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        /// Property containing the folder where files can be exported to. The folder should be
        /// public.
        /// </summary>
        string PublicExportFolder { get; }

        /// <summary>
        /// Base path to use in WebView control
        /// </summary>
        string WebViewBasePath { get; }

        /// <summary>
        /// Opens asset stream and returns it
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>stream to read from file</returns>
        Stream OpenAssetStream(string assetFilename);

        /// <summary>
        /// Loads text of asset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>text content of asset</returns>
        string LoadAssetText(string assetFilename);

        /// <summary>
        /// Loads binary data of asset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>binary content of asset</returns>
        byte[] LoadAssetBinaryData(string assetFilename);

        /// <summary>
        /// Sets app theme to use for platform
        /// </summary>
        /// <param name="requestedTheme">requested theme</param>
        void SetPlatformTheme(OSAppTheme requestedTheme);
    }
}
