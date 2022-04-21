using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace WhereToFly.App.Android
{
    /// <summary>
    /// JavaScript callback handler for calling from JavaScript to C#
    /// </summary>
    internal class JavaScriptCallbackHandler : Java.Lang.Object
    {
        /// <summary>
        /// Name of JavaScript callback object to use
        /// </summary>
        internal const string ObjectName = "callback";

        /// <summary>
        /// Weak reference to web view renderer where this handler is used
        /// </summary>
        private readonly WeakReference<WebViewRenderer> webViewRenderer;

        /// <summary>
        /// Creates a new callback handler
        /// </summary>
        /// <param name="renderer">web view renderer</param>
        public JavaScriptCallbackHandler(WebViewRenderer renderer)
        {
            this.webViewRenderer = new WeakReference<WebViewRenderer>(renderer);
        }

        /// <summary>
        /// Calls the callback handler; use in JavaScript: callback.call('action', JSON.stringify(args));
        /// </summary>
        /// <param name="action">action keyword</param>
        /// <param name="args">args in JSON format</param>
        [global::Android.Webkit.JavascriptInterface]
        [Java.Interop.Export("call")]
        public void Call(string action, string args)
        {
            if (this.webViewRenderer != null &&
                this.webViewRenderer.TryGetTarget(out WebViewRenderer renderer))
            {
                string url = $"callback://{action}/{args}";

                // send event via public (but hidden from Intellisense) method
                Device.BeginInvokeOnMainThread(() =>
                {
                    renderer?.Element?.SendNavigating(
                        new WebNavigatingEventArgs(
                            WebNavigationEvent.NewPage,
                            new UrlWebViewSource { Url = url },
                            url));
                });
            }
        }
    }
}
