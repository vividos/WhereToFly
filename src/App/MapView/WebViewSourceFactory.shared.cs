using System.Threading.Tasks;
using Xamarin.Forms;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Factory for WebViewService instances
    /// </summary>
    internal partial class WebViewSourceFactory
    {
        /// <summary>
        /// Backing store for the factory instance
        /// </summary>
        private static WebViewSourceFactory instance;

        /// <summary>
        /// Returns the web view source factory instance
        /// </summary>
        public static WebViewSourceFactory Instance
            => instance ??= new WebViewSourceFactory();

        /// <summary>
        /// Returns web view source for the MapView control
        /// </summary>
        /// <returns>web view source</returns>
        public Task<WebViewSource> GetMapViewSource()
            => this.PlatformGetMapViewSource();

        /// <summary>
        /// Returns web view source for the HeightProfileView control
        /// </summary>
        /// <returns>web view source</returns>
        public Task<WebViewSource> GetHeightProfileViewSource()
            => this.PlatformGetHeightProfileViewSource();
    }
}
