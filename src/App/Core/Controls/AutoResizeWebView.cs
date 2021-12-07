using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Controls
{
    /// <summary>
    /// Web view that auto-resizes its height to the displayed content
    /// </summary>
    public class AutoResizeWebView : WebView
    {
        /// <summary>
        /// Creates a new auto-resize web view
        /// </summary>
        public AutoResizeWebView()
        {
            this.Navigated += this.OnNavigated;
        }

        /// <summary>
        /// Called when navigation to page has finished
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnNavigated(object sender, WebNavigatedEventArgs args)
        {
            Debug.WriteLine($"OnNavigated: result={args.Result} event={args.NavigationEvent}");

            this.Navigated -= this.OnNavigated;

            string result = await this.EvaluateJavaScriptAsync("javascript:document.body.scrollHeight");

            if (double.TryParse(result, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out double height))
            {
                this.HeightRequest = height;
            }
        }
    }
}
