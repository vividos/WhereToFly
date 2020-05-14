using WhereToFly.App.Core.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(AutoResizeWebView), typeof(WhereToFly.App.UWP.UwpAutoResizeWebViewRenderer))]

namespace WhereToFly.App.UWP
{
    /// <summary>
    /// UWP custom renderer for AutoResizeWebView. This renderer provides calling the
    /// Xamarin.Forms.WebView Navigated event even when a HtmlWebViewSource is used. The event is
    /// currently not sent on UWP's Forms WebView, but on Android's implementation it is.
    /// </summary>
    public class UwpAutoResizeWebViewRenderer : WebViewRenderer
    {
        /// <summary>
        /// Called when web view element has been changed
        /// </summary>
        /// <param name="e">event args for web view change</param>
        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null &&
                this.Control != null)
            {
                this.Control.NavigationCompleted += this.OnNavigationCompleted;
            }

            if (e.OldElement != null && this.Control != null)
            {
                this.Control.NavigationCompleted -= this.OnNavigationCompleted;
            }
        }

        /// <summary>
        /// Called when the UWP WebView event NavigationCompleted has been sent.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigationCompleted(
            Windows.UI.Xaml.Controls.WebView sender,
            Windows.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs args)
        {
            if (this.Element.Source is HtmlWebViewSource)
            {
                this.Element.SendNavigated(
                    new WebNavigatedEventArgs(
                        WebNavigationEvent.NewPage,
                        this.Element.Source,
                        "about:blank", // no URL available
                        args.IsSuccess ? WebNavigationResult.Success : WebNavigationResult.Failure));
            }
        }
    }
}
