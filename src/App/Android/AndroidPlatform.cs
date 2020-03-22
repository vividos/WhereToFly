using Android.Content;
using System.IO;
using WhereToFly.App.Core;
using Xamarin.Forms;

[assembly: Dependency(typeof(WhereToFly.App.Android.AndroidPlatform))]

namespace WhereToFly.App.Android
{
    /// <summary>
    /// Platform specific functions
    /// </summary>
    public class AndroidPlatform : IPlatform
    {
        /// <summary>
        /// Returns current context, either from current activity, or the global application
        /// context
        /// </summary>
        internal static Context CurrentContext
            => Xamarin.Essentials.Platform.CurrentActivity ?? global::Android.App.Application.Context;

        /// <summary>
        /// Property containing the Android app data folder
        /// </summary>
        public string AppDataFolder
            => CurrentContext.FilesDir.AbsolutePath;

        /// <summary>
        /// Property containing the Android cache data folder
        /// </summary>
        public string CacheDataFolder
            => CurrentContext.CacheDir.AbsolutePath;

        /// <summary>
        /// Property containing the public external storage folder
        /// </summary>
        public string PublicExportFolder
        {
#pragma warning disable CS0618 // Type or member is obsolete
            get
            {
                return global::Android.OS.Environment.GetExternalStoragePublicDirectory(
                    global::Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Base path to use in WebView control, for Android
        /// </summary>
        public string WebViewBasePath => "file:///android_asset/";

        /// <summary>
        /// Opens asset stream and returns it
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>stream to read from file</returns>
        public Stream OpenAssetStream(string assetFilename)
        {
            var assetManager = CurrentContext.Assets;

            return assetManager.Open(assetFilename);
        }

        /// <summary>
        /// Loads text of Android asset file from given filename
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
    }
}
