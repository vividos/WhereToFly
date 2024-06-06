#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace WhereToFly.App.Resources
{
    /// <summary>
    /// Access to the resource's assets
    /// </summary>
    public static class Assets
    {
        /// <summary>
        /// Gets an asset as Stream
        /// </summary>
        /// <param name="assetRelativeFilename">relative path to asset file</param>
        /// <returns>stream, or null when asset file couldn't be found</returns>
        public static async Task<Stream?> Get(string assetRelativeFilename)
        {
#if ANDROID || WINDOWS
            return await Microsoft.Maui.Storage.
                FileSystem.OpenAppPackageFileAsync(assetRelativeFilename);
#else
            string dottedFilename = "WhereToFly.App.Resources.Assets." +
                assetRelativeFilename.Replace("/", ".");

            return typeof(Assets).Assembly.GetManifestResourceStream(
                dottedFilename);
#endif
        }
    }
}
