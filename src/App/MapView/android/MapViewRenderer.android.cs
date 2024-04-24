using Android.Content;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(
    typeof(WhereToFly.App.MapView.MapView),
    typeof(WhereToFly.App.MapView.MapViewRenderer))]

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Android custom WebView renderer
    /// See https://xamarinhelp.com/webview-rendering-engine-configuration/
    /// </summary>
    public class MapViewRenderer : WebViewRenderer
    {
        /// <summary>
        /// Creates a new web view renderer object
        /// </summary>
        /// <param name="context">context to pass to base class</param>
        public MapViewRenderer(Context context)
            : base(context)
        {
        }

        /// <summary>
        /// Called when web view element has been changed
        /// </summary>
        /// <param name="e">event args for web view change</param>
        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            if (this.Control == null)
            {
                return;
            }

            if (e.OldElement != null)
            {
                this.Control.RemoveJavascriptInterface(
                    JavaScriptCallbackHandler.ObjectName);
            }

            if (e.NewElement != null)
            {
                this.SetupWebViewSettings();
            }
        }

        /// <summary>
        /// Called when an element property has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event args</param>
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // fix a potential crash in Xamarin.Forms renderer that may not be
            // fixed anymore
            if (e.PropertyName == nameof(WebView.Source) &&
                this.Control == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "ignore loading Source when Control is still null");
            }
            else
            {
                base.OnElementPropertyChanged(sender, e);
            }
        }

        /// <summary>
        /// Returns web view client to use for WebView control; this is used to configure the web
        /// view client to use.
        /// </summary>
        /// <returns>created web view client</returns>
        protected override global::Android.Webkit.WebViewClient GetWebViewClient()
        {
            var mapView = this.Element as MapView;

            var webViewClient = new MapViewWebViewClient(
                this,
                mapView?.LogErrorAction);

            return webViewClient;
        }

        /// <summary>
        /// Sets up settings for WebView element
        /// </summary>
        private void SetupWebViewSettings()
        {
            // use this to debug WebView from Chrome running on PC
#if DEBUG
            global::Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
#endif
            // Note: don't put this.Control.Settings in a local variable, it doesn't work
            this.Control.Settings.JavaScriptEnabled = true;
            this.Control.AddJavascriptInterface(
                new JavaScriptCallbackHandler(this),
                JavaScriptCallbackHandler.ObjectName);

            // set secure settings
            this.Control.Settings.AllowFileAccess = false;
            this.Control.Settings.AllowContentAccess = false;
            this.Control.Settings.AllowFileAccessFromFileURLs = false;
            this.Control.Settings.AllowUniversalAccessFromFileURLs = false;

            // set up cache
            this.Control.Settings.CacheMode = global::Android.Webkit.CacheModes.Normal;
        }
    }
}
