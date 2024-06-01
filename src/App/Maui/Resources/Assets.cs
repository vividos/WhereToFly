using System;
using System.IO;
using System.Threading.Tasks;

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
#if NET7_0_OR_GREATER
            return await Microsoft.Maui.Storage.
                FileSystem.OpenAppPackageFileAsync(assetRelativeFilename);
#elif NETSTANDARD
            string dottedFilename = "WhereToFly.App.Resources.Assets." +
                assetRelativeFilename.Replace("/", ".");

            return typeof(Assets).Assembly.GetManifestResourceStream(
                dottedFilename);
#elif MONOANDROID
            return await Xamarin.Essentials.
                FileSystem.OpenAppPackageFileAsync(assetRelativeFilename);
#elif WINDOWS_UWP
            try
            {
                return await Xamarin.Essentials.
                    FileSystem.OpenAppPackageFileAsync(
                        "WhereToFly.App.Resources/Assets/" + assetRelativeFilename);
            }
            catch (Exception)
            {
                return await Xamarin.Essentials.
                    FileSystem.OpenAppPackageFileAsync(
                        "WhereToFly.App.MapView/Assets/" + assetRelativeFilename);
            }
#endif
        }
    }
}
