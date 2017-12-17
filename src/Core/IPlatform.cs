namespace WhereToFly.Core
{
    /// <summary>
    /// Platform abstraction interface
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        /// Property containing the app version number
        /// </summary>
        string AppVersionNumber { get; }

        /// <summary>
        /// Property containing the folder where the app can place its data files.
        /// </summary>
        string AppDataFolder { get; }

        /// <summary>
        /// Property containing the folder where the app can place cache files. The cache folder
        /// can always be cleard without impact the app.
        /// </summary>
        string CacheDataFolder { get; }

        /// <summary>
        /// Base path to use in WebView control
        /// </summary>
        string WebViewBasePath { get; }

        /// <summary>
        /// Loads text of asset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>text content of asset</returns>
        string LoadAssetText(string assetFilename);
    }
}
