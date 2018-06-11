using Android.Content;
using Android.OS;
using WhereToFly.App.Core;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(WebView), typeof(WhereToFly.App.Android.AndroidWebViewRenderer))]

namespace WhereToFly.App.Android
{
    /// <summary>
    /// Android custom WebView renderer
    /// See https://xamarinhelp.com/webview-rendering-engine-configuration/
    /// </summary>
    public class AndroidWebViewRenderer : WebViewRenderer
    {
        /// <summary>
        /// Creates a new web view renderer object
        /// </summary>
        /// <param name="context">context to pass to base class</param>
        public AndroidWebViewRenderer(Context context)
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

            if (this.Control != null &&
                e.NewElement != null)
            {
                this.SetupWebViewSettings();
            }
        }

        /// <summary>
        /// Sets up settings for WebView element
        /// </summary>
        private void SetupWebViewSettings()
        {
            // can only get WebViewClient on Android 8.0 (API Level 26) or higher
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var previousWebViewClient = this.Control.WebViewClient;

                var webViewClient = new AndroidWebViewClient(previousWebViewClient);
                webViewClient.CorsWebsiteHosts.Add("thermal.kk7.ch");

                this.Control.SetWebViewClient(webViewClient);
            }

            // use this to debug WebView from Chrome running on PC
#if DEBUG
            global::Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
#endif
            this.Control.Settings.JavaScriptEnabled = true;

            // enable this to ensure CesiumJS web worker are able to function
            // https://stackoverflow.com/questions/32020039/using-a-web-worker-in-a-local-file-webview
            this.Control.Settings.AllowFileAccessFromFileURLs = true;

            // this is needed to mix local content with https
            this.Control.Settings.MixedContentMode = global::Android.Webkit.MixedContentHandling.CompatibilityMode;

            // set up cache
            var platform = DependencyService.Get<IPlatform>();

            this.Control.Settings.SetAppCacheMaxSize(128 * 1024 * 1024); // 128 MB
            this.Control.Settings.SetAppCachePath(platform.CacheDataFolder);
            this.Control.Settings.SetAppCacheEnabled(true);
            this.Control.Settings.CacheMode = global::Android.Webkit.CacheModes.CacheElseNetwork;
        }
    }
}
