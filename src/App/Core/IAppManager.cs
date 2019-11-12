namespace WhereToFly.App.Core
{
    /// <summary>
    /// Interface to app manager that provides operations on external apps
    /// </summary>
    public interface IAppManager
    {
        /// <summary>
        /// Opens app with given package name
        /// </summary>
        /// <param name="packageName">package name of app to open</param>
        /// <returns>true when start was successful, false when not</returns>
        bool OpenApp(string packageName);

        /// <summary>
        /// Retrieves an icon for an app with given package name
        /// </summary>
        /// <param name="packageName">package name of app to get icon</param>
        /// <returns>image data bytes, or null when no image could be retrieved</returns>
        byte[] GetAppIcon(string packageName);
    }
}
