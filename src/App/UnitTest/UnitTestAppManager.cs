using System.Diagnostics;
using WhereToFly.App.Core;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// IAppManager implementation for unit tests
    /// </summary>
    internal class UnitTestAppManager : IAppManager
    {
        /// <summary>
        /// Indicates if methods should simulate that given app exists
        /// </summary>
        public bool AppExists { get; set; } = true;

        /// <summary>
        /// Indicates if app has been opened using the OpenApp() method
        /// </summary>
        public bool AppHasBeenOpened { get; internal set; }

        /// <inheritdoc />
        public bool IsAvailable(string packageName) => this.AppExists;

        /// <inheritdoc />
        public byte[] GetAppIcon(string packageName)
        {
            Debug.WriteLine("unit test: getting app icon for app " + packageName);

            return this.AppExists ? new byte[] { 0x12, 0x34 } : null;
        }

        /// <inheritdoc />
        public bool OpenApp(string packageName)
        {
            Debug.WriteLine("unit test: opening app " + packageName);

            this.AppHasBeenOpened = true;

            return this.AppExists;
        }
    }
}
