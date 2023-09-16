#if ANDROID
using Microsoft.Maui;

namespace WhereToFly.App.MapView
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
        /// Virtual web view
        /// </summary>
        private readonly IWebView virtualWebView;

        /// <summary>
        /// Creates a new callback handler
        /// </summary>
        /// <param name="virtualWebView">virtual web view</param>
        public JavaScriptCallbackHandler(IWebView virtualWebView)
        {
            this.virtualWebView = virtualWebView;
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
            string url = $"callback://{action}/{args}";

            // send event via public (but hidden from Intellisense) method
            this.virtualWebView.Navigating(
                WebNavigationEvent.NewPage,
                url);
        }
    }
}
#endif
