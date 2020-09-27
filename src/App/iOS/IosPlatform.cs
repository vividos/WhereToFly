using System;
using System.IO;
using WhereToFly.App.Core;
using Xamarin.Forms;

[assembly: Dependency(typeof(WhereToFly.App.iOS.IosPlatform))]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.StyleCop.CSharp.NamingRules",
    "SA1300:ElementMustBeginWithUpperCaseLetter",
    Scope = "namespace",
    Target = "~N:WhereToFly.App.iOS",
    Justification = "iOS is a proper name")]

namespace WhereToFly.App.iOS
{
    /// <summary>
    /// Platform specific functions
    /// </summary>
    public class IosPlatform : IPlatform
    {
        /// <summary>
        /// Property containing the iOS library folder
        /// </summary>
        public string AppDataFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");

        /// <summary>
        /// Property containing the iOS library cache folder
        /// </summary>
        public string CacheDataFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", "Caches");

        /// <summary>
        /// Property containing the iOS Documents folder
        /// </summary>
        public string PublicExportFolder => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /// <summary>
        /// Base path to use in WebView control, for iOS
        /// </summary>
        public string WebViewBasePath => Foundation.NSBundle.MainBundle.BundlePath + "/Assets/";

        /// <summary>
        /// Opens asset stream and returns it
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>stream to read from file</returns>
        public Stream OpenAssetStream(string assetFilename)
        {
            string assetPath = Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, "Assets");

            return new FileStream(
                Path.Combine(assetPath, assetFilename),
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
        }

        /// <summary>
        /// Loads text of asset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>text content of asset</returns>
        public string LoadAssetText(string assetFilename)
        {
            using (var stream = this.OpenAssetStream(assetFilename))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Loads binary data of asset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>binary content of asset</returns>
        public byte[] LoadAssetBinaryData(string assetFilename)
        {
            using (var stream = this.OpenAssetStream(assetFilename))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.GetBuffer();
            }
        }

        /// <summary>
        /// Sets app theme to use for platform
        /// </summary>
        /// <param name="requestedTheme">requested theme</param>
        public void SetPlatformTheme(OSAppTheme requestedTheme)
        {
            // not implemented yet
        }
    }
}
