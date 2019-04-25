using System.IO;
using WhereToFly.App.Core;
using Xamarin.Forms;

[assembly: Dependency(typeof(WhereToFly.App.UWP.UwpAppManager))]

namespace WhereToFly.App.UWP
{
    /// <summary>
    /// UWP app manager implementation that provides operations on external UWP apps.
    /// </summary>
    public class UwpAppManager : IAppManager
    {
        /// <summary>
        /// Starts app.
        /// </summary>
        /// <param name="packageName">android package name</param>
        /// <returns>true when start was successful, false when not</returns>
        public bool OpenApp(string packageName)
        {
            return false;
        }

        /// <summary>
        /// Retrieves an icon for an UWP app
        /// </summary>
        /// <param name="packageName">package name of app to get icon</param>
        /// <returns>image data bytes, or null when no image could be retrieved</returns>
        public byte[] GetAppIcon(string packageName)
        {
            return new byte[0];
        }

        /// <summary>
        /// Loads app icon for package name and returns a stream
        /// </summary>
        /// <param name="packageName">package name of app icon to load</param>
        /// <returns>stream containing a PNG image, or null when no bitmap could be loaded</returns>
        private MemoryStream LoadAppIcon(string packageName)
        {
            return null;
        }
    }
}
