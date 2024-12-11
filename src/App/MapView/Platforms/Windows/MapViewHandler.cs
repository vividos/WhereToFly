using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Handler for <see cref="MapView"/> control, Windows platform.
    /// </summary>
    internal partial class MapViewHandler : WebViewHandler
    {
        /// <summary>
        /// Called when handlers are connected to the platform view
        /// </summary>
        /// <param name="platformView">platform view</param>
        protected override void ConnectHandler(WebView2 platformView)
        {
            base.ConnectHandler(platformView);

            platformView.CoreWebView2Initialized += this.OnCoreWebView2Initialized;
            this.PlatformView.WebMessageReceived += this.OnWebMessageReceived;
        }

        /// <summary>
        /// Called when WebView2 received a web message
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnWebMessageReceived(
            WebView2 sender,
            CoreWebView2WebMessageReceivedEventArgs args)
        {
            if (this.VirtualView is IWebMessageListener listener)
            {
                listener.OnReceivedWebMessage(args.WebMessageAsJson);
            }
        }

        /// <summary>
        /// Called when handlers are disconnected from the platform view
        /// </summary>
        /// <param name="platformView">platform view</param>
        protected override void DisconnectHandler(WebView2 platformView)
        {
            platformView.WebMessageReceived -= this.OnWebMessageReceived;
            platformView.CoreWebView2Initialized -= this.OnCoreWebView2Initialized;

            base.DisconnectHandler(platformView);
        }

        /// <summary>
        /// Called when the WebView2 control has been initialized successfully
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnCoreWebView2Initialized(
            WebView2 sender,
            CoreWebView2InitializedEventArgs args)
        {
            this.PlatformView.CoreWebView2Initialized -=
                this.OnCoreWebView2Initialized;

            SetupWebViewSettings(sender.CoreWebView2.Settings);
        }

        /// <summary>
        /// Sets up WebView2 settings
        /// </summary>
        /// <param name="settings">settings object</param>
        private static void SetupWebViewSettings(CoreWebView2Settings settings)
        {
#if DEBUG
            // press F12 when the WebView has focus to start dev tools
            settings.AreDevToolsEnabled = true;
            settings.AreBrowserAcceleratorKeysEnabled = true;
#else
            settings.AreDevToolsEnabled = false;
            settings.AreBrowserAcceleratorKeysEnabled = false;
#endif
            settings.IsWebMessageEnabled = true;

            settings.AreDefaultContextMenusEnabled = false;
            settings.AreDefaultScriptDialogsEnabled = false;
            settings.AreHostObjectsAllowed = false;
            settings.IsGeneralAutofillEnabled = false;
            settings.IsPasswordAutosaveEnabled = false;
            settings.IsPinchZoomEnabled = false;
            settings.IsStatusBarEnabled = false;
            settings.IsSwipeNavigationEnabled = false;
            settings.IsZoomControlEnabled = false;

            string userAgent = settings.UserAgent;
            userAgent += $" WebViewApp {AppInfo.Name.Replace(' ', '-')}/{AppInfo.VersionString}";
            settings.UserAgent = userAgent;
        }
    }
}
