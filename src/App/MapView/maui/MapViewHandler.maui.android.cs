#if ANDROID
using Microsoft.Maui.Handlers;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Handler for <see cref="MapView"/> control, Android platform.
    /// </summary>
    internal partial class MapViewHandler : WebViewHandler
    {
        /// <summary>
        /// Called when handlers are connected to the platform view
        /// </summary>
        /// <param name="platformView">platform view</param>
        protected override void ConnectHandler(Android.Webkit.WebView platformView)
        {
            base.ConnectHandler(platformView);

            this.SetupWebViewSettings(platformView, this.VirtualView);
        }

        /// <summary>
        /// Called when handlers are disconnected from the platform view
        /// </summary>
        /// <param name="platformView">platform view</param>
        protected override void DisconnectHandler(Android.Webkit.WebView platformView)
        {
            base.DisconnectHandler(platformView);

            platformView.RemoveJavascriptInterface(
                JavaScriptCallbackHandler.ObjectName);
        }

        /// <summary>
        /// Sets up settings for WebView element
        /// </summary>
        /// <param name="platformView">Android WebView</param>
        /// <param name="virtualView">Maui WebView</param>
        private void SetupWebViewSettings(
            Android.Webkit.WebView platformView,
            Microsoft.Maui.IWebView virtualView)
        {
            var mapView = virtualView as MapView;

            var context = platformView.Context ?? Android.App.Application.Context;

            platformView.SetWebViewClient(
                new MapViewWebViewClient(
                    context,
                    this,
                    mapView?.LogErrorAction));

            // use this to debug WebView from Chrome running on PC
#if DEBUG
            Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
#endif
            // Note: don't put platformView.Settings in a local variable, it doesn't work
            platformView.Settings.JavaScriptEnabled = true;

            platformView.AddJavascriptInterface(
                new JavaScriptCallbackHandler(virtualView),
                JavaScriptCallbackHandler.ObjectName);

            // set secure settings
            platformView.Settings.AllowFileAccess = false;
            platformView.Settings.AllowContentAccess = false;
            platformView.Settings.AllowFileAccessFromFileURLs = false;
            platformView.Settings.AllowUniversalAccessFromFileURLs = false;

            // set up cache
            platformView.Settings.CacheMode =
                Android.Webkit.CacheModes.Normal;
        }
    }
}
#endif
