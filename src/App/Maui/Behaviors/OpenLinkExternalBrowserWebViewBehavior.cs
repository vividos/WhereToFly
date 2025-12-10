#pragma warning disable S1118 // Utility classes should not have public constructors

namespace WhereToFly.App.Behaviors;

/// <summary>
/// Behavior that can be attached to a <see cref="WebView"/> to open web links in an external
/// browser
/// </summary>
internal partial class OpenLinkExternalBrowserWebViewBehavior
{
    /// <summary>
    /// Called when the web view navigates to a new URL
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="args">event args</param>
    private static void OnNavigating(object? sender, WebNavigatingEventArgs args)
    {
        if (args.NavigationEvent == WebNavigationEvent.NewPage &&
            (args.Url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) ||
            args.Url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)) &&
            !args.Url.StartsWith("https://appdir/", StringComparison.InvariantCultureIgnoreCase))
        {
            args.Cancel = true;

            OpenLink(args.Url);
        }
    }

    /// <summary>
    /// Opens link in external browser window
    /// </summary>
    /// <param name="url">link URL to open</param>
    private static void OpenLink(string url)
    {
        MainThread.BeginInvokeOnMainThread(
            () =>
            {
                try
                {
                    Browser.OpenAsync(
                        url,
                        BrowserLaunchMode.External);
                }
                catch (Exception ex)
                {
                    App.LogError(ex);
                }
            });
    }
}
