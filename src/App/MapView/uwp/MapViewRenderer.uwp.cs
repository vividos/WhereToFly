using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.App.MapView;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(MapView), typeof(WhereToFly.App.MapView.MapViewRenderer))]
[assembly: ExportRenderer(typeof(HeightProfileWebView), typeof(WhereToFly.App.MapView.MapViewRenderer))]

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// UWP custom Forms WebView renderer that uses WebView2
    /// See https://xamarinhelp.com/webview-rendering-engine-configuration/
    /// and https://stackoverflow.com/questions/67067055/can-i-use-webview2-in-xamarin-forms-i-need-to-use-it-in-android-ios-windows
    /// </summary>
    public class MapViewRenderer :
        ViewRenderer<WebView, WebView2>,
        IWebViewDelegate
    {
        /// <summary>
        /// Hostname for virtual mapping to local app folder
        /// </summary>
        private const string LocalAppHostname = "localapp";

        /// <summary>
        /// Called when web view element has been changed
        /// </summary>
        /// <param name="e">event args for web view change</param>
        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                var oldElement = e.OldElement;
                oldElement.EvalRequested -= this.OnEvalRequested;
                oldElement.EvaluateJavaScriptRequested -= this.OnEvaluateJavaScriptRequested;
            }

            if (e.NewElement != null)
            {
                if (this.Control == null)
                {
                    var webView = new WebView2();
                    this.SetupWebView(webView);
                    this.SetNativeControl(webView);
                }

                var newElement = e.NewElement;
                newElement.EvalRequested += this.OnEvalRequested;
                newElement.EvaluateJavaScriptRequested += this.OnEvaluateJavaScriptRequested;

                if (newElement.Source != null)
                {
                    newElement.Source.Load(this);
                }
            }
        }

        /// <summary>
        /// Called when a property of the Forms element has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event args</param>
        protected override void OnElementPropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == WebView.SourceProperty.PropertyName)
            {
                // load via IWebViewDelegate implementation
                this.Element.Source?.Load(this);
            }
        }

        /// <summary>
        /// Sets up web view, registering event handler
        /// </summary>
        /// <param name="webView">web view to set up</param>
        private void SetupWebView(WebView2 webView)
        {
            if (webView == null)
            {
                return;
            }

            webView.CoreWebView2Initialized += this.OnCoreWebView2Initialized;
            webView.CoreProcessFailed += this.OnCoreProcessFailed;
            webView.NavigationStarting += this.OnNavigationStarting;
            webView.NavigationCompleted += this.OnNavigationCompleted;
            webView.WebMessageReceived += this.OnWebMessageReceived;
        }

        /// <summary>
        /// Cleans up web view, unregistering event handler
        /// </summary>
        /// <param name="webView">web view to clean up</param>
        private void CleanupWebView(WebView2 webView)
        {
            if (webView == null)
            {
                return;
            }

            webView.CoreWebView2Initialized -= this.OnCoreWebView2Initialized;
            webView.CoreProcessFailed -= this.OnCoreProcessFailed;
            webView.NavigationStarting -= this.OnNavigationStarting;
            webView.NavigationCompleted -= this.OnNavigationCompleted;
            webView.WebMessageReceived -= this.OnWebMessageReceived;
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
            var settings = sender.CoreWebView2.Settings;

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

            if (this.Element?.Source is UrlWebViewSource urlWebViewSource)
            {
                this.SetupVirtualAppFolder(urlWebViewSource.Url);
            }
        }

        /// <summary>
        /// Called when the WebView 2 process has a failure
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnCoreProcessFailed(
            WebView2 sender,
            CoreWebView2ProcessFailedEventArgs args)
        {
            Debug.WriteLine($"WebView2: core process failed: {args.Reason}");
        }

        /// <summary>
        /// Called when navigation in WebView2 control is about to start
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigationStarting(
            WebView2 sender,
            CoreWebView2NavigationStartingEventArgs args)
        {
            string url = args.Uri;
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            if (url.StartsWith("callback://", StringComparison.InvariantCultureIgnoreCase))
            {
                args.Cancel = true;
            }

            var sendNavigatingArgs = new WebNavigatingEventArgs(
                WebNavigationEvent.NewPage,
                new UrlWebViewSource { Url = url },
                url);

            this.Element.SendNavigating(sendNavigatingArgs);
        }

        /// <summary>
        /// Sets up virtual app folder to access locally stored pages
        /// </summary>
        /// <param name="url">URL to check if virtual app folder should be set up or not</param>
        private void SetupVirtualAppFolder(string url)
        {
            const string LocalAppUrlScheme = $"https://{LocalAppHostname}/";
            if (url.ToLowerInvariant().StartsWith(LocalAppUrlScheme.ToLowerInvariant()))
            {
                // no ms-appx-web:// support in WebView2, but allow access to local app data
                // see also: https://github.com/dotnet/maui/pull/7672
                string localAppFolder =
                    Windows.ApplicationModel.Package.Current.InstalledLocation.Path;

                this.Control.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    LocalAppHostname,
                    localAppFolder,
                    CoreWebView2HostResourceAccessKind.Allow);
            }
            else
            {
                this.Control.CoreWebView2.ClearVirtualHostNameToFolderMapping(LocalAppHostname);
            }
        }

        /// <summary>
        /// Called when navigation in WebView2 control has completed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigationCompleted(
            WebView2 sender,
            CoreWebView2NavigationCompletedEventArgs args)
        {
            string url = sender.Source.AbsoluteUri;

            var result = args.IsSuccess
                ? WebNavigationResult.Success
                : WebNavigationResult.Failure;

            this.Element.SendNavigated(
                new WebNavigatedEventArgs(
                    WebNavigationEvent.NewPage,
                    new UrlWebViewSource
                    {
                        Url = url,
                    },
                    url,
                    result));
        }

        /// <summary>
        /// Called when the Forms WebView method Eval() is called
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnEvalRequested(object sender, EvalRequested args)
        {
            await this.Control.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                    try
                    {
                        await this.Control.ExecuteScriptAsync(args.Script);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"WebView2: Error while executing Javascrpt: {ex} Javascript: {args.Script}");
                    }
                });
        }

        /// <summary>
        /// Called when the EvaluateJavaScriptAsync() method is called
        /// </summary>
        /// <param name="javascript">Javascript code to execute</param>
        /// <returns>return value as JSON</returns>
        private async Task<string> OnEvaluateJavaScriptRequested(string javascript)
        {
            return await this.Control.ExecuteScriptAsync(javascript);
        }

        /// <summary>
        /// Called when a web message has been received from JavaScript, by calling
        /// window.chrome.webview.postMessage(...)
        /// The message is packaged into a callback:// link with the "webMessage" verb.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnWebMessageReceived(
            WebView2 sender,
            CoreWebView2WebMessageReceivedEventArgs args)
        {
            Debug.WriteLine($"WebMessageReceived: {sender}, Source={args.Source}, WebMessage={args.WebMessageAsJson}");

#pragma warning disable S1075 // URIs should not be hardcoded
            string url = "callback://webMessage/" + args.WebMessageAsJson;
#pragma warning restore S1075 // URIs should not be hardcoded

            // send event via public (but hidden from Intellisense) method
            Device.BeginInvokeOnMainThread(() =>
            {
                this.Element.SendNavigating(
                    new WebNavigatingEventArgs(
                        WebNavigationEvent.NewPage,
                        new UrlWebViewSource { Url = url },
                        url));
            });
        }

        /// <summary>
        /// Disposes of managed resources of the renderer
        /// </summary>
        /// <param name="disposing">
        /// true when called from Dispose(), false when called from finalizer
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.CleanupWebView(this.Control);

                if (this.Element != null)
                {
                    this.Element.EvalRequested -= this.OnEvalRequested;
                    this.Element.EvaluateJavaScriptRequested -= this.OnEvaluateJavaScriptRequested;
                }
            }

            base.Dispose(disposing);
        }

        #region IWebViewDelegate implementation

        /// <summary>
        /// Loads web view content using HTML code
        /// </summary>
        /// <param name="html">HTML code</param>
        /// <param name="baseUrl">base URL; currently ignored</param>
        public void LoadHtml(string html, string baseUrl)
        {
            //// doesn't work: this.Control.BaseUri = new Uri(baseUrl);
            this.Control.NavigateToString(html);
        }

        /// <summary>
        /// Loads web view content using URL
        /// </summary>
        /// <param name="url">URL to load</param>
        public void LoadUrl(string url)
        {
            Debug.Assert(
                !url.Contains("ms-appx-web"),
                "ms-appx-web not supported by WebView2!");

            if (this.Control?.CoreWebView2 != null)
            {
                this.SetupVirtualAppFolder(url);
            }

            if (this.Control != null)
            {
                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                this.Control.Source = uri;
            }
        }
        #endregion
    }
}
