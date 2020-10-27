using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(WebView), typeof(WhereToFly.App.UWP.UwpWebViewRenderer))]

namespace WhereToFly.App.UWP
{
    /// <summary>
    /// UWP custom WebView renderer
    /// See https://xamarinhelp.com/webview-rendering-engine-configuration/
    /// </summary>
    public class UwpWebViewRenderer : WebViewRenderer
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
                this.SetupWebViewSettings();
            }
        }

        /// <summary>
        /// Sets up settings for WebView element
        /// </summary>
        private void SetupWebViewSettings()
        {
            this.Control.Settings.IsJavaScriptEnabled = true;

            this.Control.ScriptNotify += this.OnScriptNotify;
        }

        /// <summary>
        /// Called when window.script.notify has been called from JavaScript
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnScriptNotify(object sender, Windows.UI.Xaml.Controls.NotifyEventArgs args)
        {
            ////Debug.WriteLine($"ScriptNotify: {sender}, CallingUri={args.CallingUri}, Value={args.Value}");

            string url = args.Value;

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
    }
}
