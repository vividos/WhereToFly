using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Handler for callback:// Uri schemas that can be used in a WebView to call back to the C#
    /// part of the app.
    /// </summary>
    internal class WebViewCallbackSchemaHandler
    {
        /// <summary>
        /// Callback Uri scheme to use
        /// </summary>
        private const string CallbackSchema = "callback://";

        /// <summary>
        /// Handler map to map function names to actions
        /// </summary>
        private readonly Dictionary<string, Action<string>> handlerMap =
            new();

        /// <summary>
        /// Creates a new web view callback schema handler; attaches itself to the given WebView
        /// </summary>
        /// <param name="webView">web view to attach to</param>
        public WebViewCallbackSchemaHandler(WebView webView)
        {
            webView.Navigating += this.OnNavigating_WebView;
        }

        /// <summary>
        /// Registers a new handler for a function name and an action that handles the callback
        /// </summary>
        /// <param name="functionName">function name</param>
        /// <param name="handler">handler action</param>
        public void RegisterHandler(string functionName, Action<string> handler)
        {
            this.handlerMap.Add(functionName, handler);
        }

        /// <summary>
        /// Called when web view navigates to a new URL; used to bypass callback:// URLs.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigating_WebView(object sender, WebNavigatingEventArgs args)
        {
            if (!args.Url.ToString().StartsWith(CallbackSchema))
            {
                return;
            }

            args.Cancel = true;

            string callbackParams = args.Url.ToString().Substring(CallbackSchema.Length);

            int pos = callbackParams.IndexOf('/');
            Debug.Assert(pos > 0, "callback Uri must contain a slash after the function name");

            string functionName = callbackParams.Substring(0, pos);
            string jsonParameters = callbackParams.Substring(pos + 1);

            jsonParameters = System.Net.WebUtility.UrlDecode(jsonParameters);

            if (this.handlerMap.TryGetValue(functionName, out Action<string> handler) &&
                handler != null)
            {
                handler.Invoke(jsonParameters);
            }
            else
            {
                Debug.Assert(false, "called non-registered callback function with function name " + functionName);
            }
        }
    }
}
