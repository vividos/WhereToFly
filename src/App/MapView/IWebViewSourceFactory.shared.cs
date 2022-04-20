using Xamarin.Forms;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WhereToFly.App.Core")]

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Factory for WebViewSource objects for the map view controls contained in the weblib npm
    /// project.
    /// </summary>
    public interface IWebViewSourceFactory
    {
        /// <summary>
        /// Returns web view source for the MapView control
        /// </summary>
        /// <returns>web view source</returns>
        WebViewSource GetMapViewSource();

        /// <summary>
        /// Returns web view source for the HeightProfileView control
        /// </summary>
        /// <returns>web view source</returns>
        WebViewSource GetHeightProfileViewSource();
    }
}
