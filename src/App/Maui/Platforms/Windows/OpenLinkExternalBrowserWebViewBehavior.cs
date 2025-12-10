using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace WhereToFly.App.Behaviors;

/// <summary>
/// Behavior that can be attached to a <see cref="WebView"/> to open web links in an external
/// browser
/// </summary>
internal partial class OpenLinkExternalBrowserWebViewBehavior :
    PlatformBehavior<WebView, WebView2>
{
    /// <summary>
    /// Called when the behavior is attached to a web view
    /// </summary>
    /// <param name="webView">web view</param>
    /// <param name="platformView">platform view</param>
    protected override void OnAttachedTo(
        WebView webView,
        WebView2 platformView)
    {
        base.OnAttachedTo(webView, platformView);

        webView.Navigating += OnNavigating;

        MainThread.BeginInvokeOnMainThread(
            async () => await InitEventHandler(platformView));
    }

    /// <summary>
    /// Initializes the event handler for WebView2
    /// </summary>
    /// <param name="platformView">web view to init event handler for</param>
    /// <returns>task to wait on</returns>
    private static async Task InitEventHandler(WebView2 platformView)
    {
        await platformView.EnsureCoreWebView2Async();

        var coreWebView2 = platformView.CoreWebView2;
        coreWebView2.NewWindowRequested += OnNewWindowRequested;
    }

    /// <summary>
    /// Called when the behavior was detached from a web view
    /// </summary>
    /// <param name="webView">web view</param>
    /// <param name="platformView">platform view</param>
    protected override void OnDetachedFrom(
        WebView webView,
        WebView2 platformView)
    {
        webView.Navigating -= OnNavigating;

        var coreWebView2 = platformView.CoreWebView2;
        if (coreWebView2 != null)
        {
            coreWebView2.NewWindowRequested -= OnNewWindowRequested;
        }

        base.OnDetachedFrom(
            webView,
            platformView);
    }

    /// <summary>
    /// Called when WebView2 wants to open a new window, e.g. from a link with target="_blank"
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="args">event args</param>
    private static void OnNewWindowRequested(
        CoreWebView2 sender,
        CoreWebView2NewWindowRequestedEventArgs args)
    {
        args.Handled = true;

        OpenLink(args.Uri);
    }
}
