namespace WhereToFly.App.Behaviors
{
    /// <summary>
    /// Behavior that can be attached to a <see cref="WebView"/> to open web links in an external
    /// browser
    /// </summary>
    internal class OpenLinkExternalBrowserWebViewBehavior : Behavior<WebView>
    {
        /// <summary>
        /// Called when the behavior is attached to a web view
        /// </summary>
        /// <param name="webView">web view</param>
        protected override void OnAttachedTo(WebView webView)
        {
            base.OnAttachedTo(webView);

            webView.Navigating += OnNavigating;
        }

        /// <summary>
        /// Called when the behavior is detaching from a web view
        /// </summary>
        /// <param name="webView">web view</param>
        protected override void OnDetachingFrom(WebView webView)
        {
            webView.Navigating -= OnNavigating;

            base.OnDetachingFrom(webView);
        }

        /// <summary>
        /// Called when the web view navigates to a new URL
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private static void OnNavigating(object sender, WebNavigatingEventArgs args)
        {
            if (args.NavigationEvent == WebNavigationEvent.NewPage &&
                args.Url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) &&
                !args.Url.StartsWith("https://appdir/", StringComparison.InvariantCultureIgnoreCase))
            {
                args.Cancel = true;

                MainThread.BeginInvokeOnMainThread(
                    () => Browser.OpenAsync(
                        args.Url,
                        BrowserLaunchMode.External));
            }
        }
    }
}
