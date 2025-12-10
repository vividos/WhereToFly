using AWebView = global::Android.Webkit.WebView;

namespace WhereToFly.App.Behaviors;

/// <summary>
/// Behavior that can be attached to a <see cref="WebView"/> to open web links in an external
/// browser
/// </summary>
internal partial class OpenLinkExternalBrowserWebViewBehavior :
    PlatformBehavior<WebView, AWebView>
{
    /// <summary>
    /// Called when the behavior is attached to a web view
    /// </summary>
    /// <param name="webView">web view</param>
    /// <param name="platformView">platform view</param>
    protected override void OnAttachedTo(
        WebView webView,
        AWebView platformView)
    {
        base.OnAttachedTo(webView, platformView);

        webView.Navigating += OnNavigating;

        // prevent opening new windows when using target="_blank"
        // .NET MAUI sets this setting to true in its handler
        platformView.Settings.SetSupportMultipleWindows(false);
    }

    /// <summary>
    /// Called when the behavior was detached from a web view
    /// </summary>
    /// <param name="webView">web view</param>
    /// <param name="platformView">platform view</param>
    protected override void OnDetachedFrom(
        WebView webView,
        AWebView platformView)
    {
        webView.Navigating -= OnNavigating;

        base.OnDetachedFrom(webView, platformView);
    }
}
