using Android.App;
using Android.Content;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using System;
using System.Linq;
using WhereToFly.App.Core.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(WeatherWebView), typeof(WhereToFly.App.Android.AndroidWeatherWebViewRenderer))]

namespace WhereToFly.App.Android
{
    /// <summary>
    /// Renderer for WeatherWebView class on Android. The renderer provides long-tapping on image
    /// links, shows a prompt to download the image and then hands off to the Android's download
    /// manager. Can also download images that need authentication, e.g. Alptherm.
    /// </summary>
    public class AndroidWeatherWebViewRenderer : WebViewRenderer
    {
        /// <summary>
        /// Gesture detector to detect long taps
        /// </summary>
        private GestureDetector gestureDetector;

        /// <summary>
        /// Creates a new renderer for the WeatherWebView class
        /// </summary>
        /// <param name="context">android context</param>
        public AndroidWeatherWebViewRenderer(Context context)
            : base(context)
        {
        }

        /// <summary>
        /// Called when the web view control is created or destroyed
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                this.Control.Touch -= this.OnTouch;

                if (this.gestureDetector != null)
                {
                    this.gestureDetector.Dispose();
                    this.gestureDetector = null;
                }
            }

            if (this.Control != null &&
                e.NewElement != null)
            {
                this.Control.Settings.BuiltInZoomControls = true;

                var longTapListener = new LongTapGestureListener();
                longTapListener.LongTap += this.OnLongTap;

                this.gestureDetector = new GestureDetector(
                    this.Context,
                    longTapListener);

                this.Control.Touch += this.OnTouch;
            }
        }

        /// <summary>
        /// Called when a gesture detector detected a touch event
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnTouch(object sender, TouchEventArgs args)
        {
            args.Handled = false;
            this.gestureDetector?.OnTouchEvent(args.Event);
        }

        /// <summary>
        /// Called when the long tap gesture listener detected a long tap
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnLongTap(object sender, GestureDetector.LongPressEventArgs args)
        {
            var hitTestResult = this.Control.GetHitTestResult();

            System.Diagnostics.Debug.WriteLine($"HitTestResult: Type={hitTestResult.Type} Extra={hitTestResult.Extra}");

            if (hitTestResult.Type != HitTestResult.ImageType &&
                hitTestResult.Type != HitTestResult.SrcImageAnchorType)
            {
                return;
            }

            string imageLink = hitTestResult.Extra;

            var baseUri = new Uri(this.Control.Url);
            var uri = new Uri(baseUri, imageLink);

            this.AskUserDownload(uri.AbsoluteUri);
        }

        /// <summary>
        /// Asks user if the given image should be downloaded
        /// </summary>
        /// <param name="imageLink">image weblink to download</param>
        private void AskUserDownload(string imageLink)
        {
            var uri = new Uri(imageLink);
            string lastSegment = uri.Segments.LastOrDefault();
            string title = lastSegment != null ? "Image: " + lastSegment : "Image";
            string filename = lastSegment ?? "image";

            var builder = new AlertDialog.Builder(this.Context);
            builder.SetTitle(title);
            builder.SetNegativeButton("Cancel", (sender1, args1) => { });
            builder.SetItems(new string[] { "Download" }, (sender2, args2) => this.StartDownload(imageLink, filename));
            builder.Show();
        }

        /// <summary>
        /// Starts download of given URL
        /// </summary>
        /// <param name="url">URL of file to download</param>
        /// <param name="filename">filename of file to download</param>
        private void StartDownload(string url, string filename)
        {
            var uri = global::Android.Net.Uri.Parse(url);
            var request = new DownloadManager.Request(uri);
            request.SetTitle(filename);
            request.SetDescription("Downloading file...");

            var cookie = CookieManager.Instance.GetCookie(url);
            request.AddRequestHeader("Cookie", cookie);

            if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.Q)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                request.AllowScanningByMediaScanner();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                request.SetDestinationInExternalPublicDir(
                    global::Android.OS.Environment.DirectoryDownloads,
                    filename);
            }

            request.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);

            var downloadManager = (DownloadManager)this.Context.GetSystemService(Context.DownloadService);
            downloadManager.Enqueue(request);

            Toast.MakeText(this.Context, "Image download started...", ToastLength.Short).Show();
        }

        #region IDisposable Support
        /// <summary>
        /// Disposes of managed and unmanaged resources
        /// </summary>
        /// <param name="disposing">
        /// true when called from Dispose(), false when called from finalizer
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing &&
                this.gestureDetector != null)
            {
                this.gestureDetector.Dispose();
                this.gestureDetector = null;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
