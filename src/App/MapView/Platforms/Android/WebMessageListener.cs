using Android.Net;
using Android.Webkit;
using AndroidX.WebKit;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Web message listener implementation for Android WebKit
    /// </summary>
    internal class WebMessageListener :
        Java.Lang.Object,
        WebViewCompat.IWebMessageListener
    {
        /// <summary>
        /// Name of JavaScript callback object to use
        /// </summary>
        internal const string ObjectName = "callback";

        /// <summary>
        /// Platform independent web message listener interface
        /// </summary>
        private readonly IWebMessageListener webMessageListener;

        /// <summary>
        /// Creates a new Android WebKit web message listener that forwards
        /// </summary>
        /// <param name="webMessageListener">web message listener interface</param>
        public WebMessageListener(IWebMessageListener webMessageListener)
        {
            this.webMessageListener = webMessageListener;
        }

        /// <summary>
        /// Called when JavaScript code calls postMessage().
        /// </summary>
        /// <param name="webView">web view</param>
        /// <param name="message">web message</param>
        /// <param name="sourceOrigin">message source origin URI</param>
        /// <param name="isMainFrame">true if message is from main frame</param>
        /// <param name="replyProy">reply proy; unused</param>
#pragma warning disable S927 // Parameter names should match base declaration and other partial definitions
        public void OnPostMessage(
            WebView webView,
            WebMessageCompat message,
            Uri sourceOrigin,
            bool isMainFrame,
            JavaScriptReplyProxy replyProy)
#pragma warning restore S927 // Parameter names should match base declaration and other partial definitions
        {
            this.webMessageListener.OnReceivedWebMessage(
                message.Data ?? "{}");
        }
    }
}
