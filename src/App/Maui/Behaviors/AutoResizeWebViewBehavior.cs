using System.Diagnostics;
using System.Globalization;

namespace WhereToFly.App.Behaviors
{
    /// <summary>
    /// Behavior that can be attached to a <see cref="WebView"/> that auto-resizes its height to
    /// the displayed content
    /// </summary>
    internal class AutoResizeWebViewBehavior : Behavior<WebView>
    {
        /// <summary>
        /// Called when the behavior is attached to a web view
        /// </summary>
        /// <param name="webView">web view</param>
        protected override void OnAttachedTo(WebView webView)
        {
            base.OnAttachedTo(webView);

            webView.Navigated += OnNavigated;
        }

        /// <summary>
        /// Called when the behavior is detaching from a web view
        /// </summary>
        /// <param name="webView">web view</param>
        protected override void OnDetachingFrom(WebView webView)
        {
            webView.Navigated -= OnNavigated;

            base.OnDetachingFrom(webView);
        }

        /// <summary>
        /// Called when navigation to page has finished
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private static void OnNavigated(object? sender, WebNavigatedEventArgs args)
        {
            if (sender is not WebView webView)
            {
                return;
            }

            Debug.WriteLine($"OnNavigated: result={args.Result} event={args.NavigationEvent}");

            webView.Navigated -= OnNavigated;

            webView.Dispatcher.Dispatch(
                async () => await ResizeWebView(webView));
        }

        /// <summary>
        /// Resizes web view to current scroll height of loaded document
        /// </summary>
        /// <returns>task to wait on</returns>
        private static async Task ResizeWebView(WebView webView)
        {
            string result =
                await webView.EvaluateJavaScriptAsync(
                    "javascript:document.body.scrollHeight");

            if (double.TryParse(
                result,
                NumberStyles.Float,
                NumberFormatInfo.InvariantInfo,
                out double height))
            {
                webView.HeightRequest = height;
            }
        }
    }
}
