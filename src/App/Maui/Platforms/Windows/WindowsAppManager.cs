using WhereToFly.App.Abstractions;
using WhereToFly.App.Platforms.Windows;

[assembly: Dependency(typeof(WindowsAppManager))]

namespace WhereToFly.App.Platforms.Windows
{
    /// <summary>
    /// Windows app manager implementation that provides operations on external Windows apps.
    /// </summary>
    public class WindowsAppManager : IAppManager
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
        public byte[]? GetAppIcon(string packageName)
        {
            return Array.Empty<byte>();
        }
    }
}
