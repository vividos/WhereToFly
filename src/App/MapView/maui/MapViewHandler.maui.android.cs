#if ANDROID
using Microsoft.Maui.Handlers;
using static Google.Android.Material.Tabs.TabLayout;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Handler for <see cref="MapView"/> control, Android platform.
    /// </summary>
    internal partial class MapViewHandler : WebViewHandler
    {
        /// <summary>
        /// Called to create platform view
        /// </summary>
        /// <returns>Android WebView</returns>
        protected override Android.Webkit.WebView CreatePlatformView()
        {
            var webView = base.CreatePlatformView();
            return webView;
        }

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

            platformView.SetWebViewClient(
                new MapViewWebViewClient(
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

            // enable this to ensure CesiumJS web worker are able to function
            // https://stackoverflow.com/questions/32020039/using-a-web-worker-in-a-local-file-webview
            platformView.Settings.AllowFileAccessFromFileURLs = true;

            // this is needed to mix local content with https
            platformView.Settings.MixedContentMode =
                Android.Webkit.MixedContentHandling.CompatibilityMode;

            // set up cache
            platformView.Settings.CacheMode =
                Android.Webkit.CacheModes.Normal;
        }
    }
}
#endif
