using System;
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
        /// Determines if the app with given package name is available
        /// </summary>
        /// <param name="packageName">package name of app to check</param>
        /// <returns>true when available, or false when not</returns>
        public bool IsAvailable(string packageName)
        {
            return false;
        }

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
            return Array.Empty<byte>();
        }
    }
}
