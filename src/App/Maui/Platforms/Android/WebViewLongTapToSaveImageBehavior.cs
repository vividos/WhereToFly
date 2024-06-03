using Android.App;
using Android.Content;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using WhereToFly.App.Android;
using WebView = global::Android.Webkit.WebView;

namespace WhereToFly.App.Behaviors
{
    /// <summary>
    /// Behavior for Android <see cref="WebView"/>. The behavior provides long-tapping on image
    /// links, shows a prompt to download the image and then hands off to the Android's download
    /// manager. Can also download images that need authentication, e.g. Alptherm.
    /// </summary>
    internal partial class WebViewLongTapToSaveImageBehavior :
        PlatformBehavior<Microsoft.Maui.Controls.WebView, WebView>
    {
        /// <summary>
        /// The web view currently attached to
        /// </summary>
        private WebView? webView;

        /// <summary>
        /// Gesture detector to detect long taps
        /// </summary>
        private GestureDetector? gestureDetector;

        /// <summary>
        /// Called when the behavior attaches to the web view
        /// </summary>
        /// <param name="bindable">Maui WebView</param>
        /// <param name="platformView">Webkit WebView</param>
        protected override void OnAttachedTo(
            Microsoft.Maui.Controls.WebView bindable,
            WebView platformView)
        {
            base.OnAttachedTo(bindable, platformView);

            this.webView = platformView;

            this.SetupWebView();
        }

        /// <summary>
        /// Called when the behavior detaches from the web view
        /// </summary>
        /// <param name="bindable">Maui WebView</param>
        /// <param name="platformView">Webkit WebView</param>
        protected override void OnDetachedFrom(
            Microsoft.Maui.Controls.WebView bindable,
            WebView platformView)
        {
            base.OnDetachedFrom(bindable, platformView);

            this.CleanupWebView();

            this.webView = null;
        }

        /// <summary>
        /// Sets up web view for behavior
        /// </summary>
        private void SetupWebView()
        {
            if (this.webView == null)
            {
                return;
            }

            this.webView.Settings.BuiltInZoomControls = true;

            var longTapListener = new LongTapGestureListener();
            longTapListener.LongTap += this.OnLongTap;

            this.gestureDetector = new GestureDetector(
                this.webView.Context,
                longTapListener);

            this.webView.Touch += this.OnTouch;
        }

        /// <summary>
        /// Cleans up behavior by detaching from event handlers
        /// </summary>
        private void CleanupWebView()
        {
            if (this.webView == null)
            {
                return;
            }

            this.webView.Touch -= this.OnTouch;

            if (this.gestureDetector != null)
            {
                this.gestureDetector.Dispose();
                this.gestureDetector = null;
            }
        }

        /// <summary>
        /// Called when a gesture detector detected a touch event
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnTouch(object? sender, global::Android.Views.View.TouchEventArgs args)
        {
            args.Handled = false;

            if (args.Event != null)
            {
                this.gestureDetector?.OnTouchEvent(args.Event);
            }
        }

        /// <summary>
        /// Called when the long tap gesture listener detected a long tap
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnLongTap(object? sender, GestureDetector.LongPressEventArgs args)
        {
            if (this.webView == null ||
                this.webView.Url == null)
            {
                return;
            }

            var hitTestResult = this.webView.GetHitTestResult();
            if (hitTestResult == null)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine(
                $"HitTestResult: Type={hitTestResult.Type} Extra={hitTestResult.Extra}");

            if (hitTestResult.Type != HitTestResult.ImageType &&
                hitTestResult.Type != HitTestResult.SrcImageAnchorType)
            {
                return;
            }

            string? imageLink = hitTestResult.Extra;

            var baseUri = new Uri(this.webView.Url);
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
            string? lastSegment = uri.Segments.LastOrDefault();

            string title = lastSegment != null
                ? "Image: " + lastSegment
                : "Image";

            string filename = lastSegment ?? "image";

            var context = this.webView?.Context
                ?? global::Android.App.Application.Context;

            var builder = new AlertDialog.Builder(context);
            builder.SetTitle(title);

            builder.SetNegativeButton(
                "Cancel",
                (sender1, args1) => { });

            builder.SetItems(
                new string[] { "Download" },
                (sender2, args2) => this.StartDownload(imageLink, filename));

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

            string? cookie = CookieManager.Instance?.GetCookie(url);
            if (!string.IsNullOrEmpty(cookie))
            {
                request.AddRequestHeader("Cookie", cookie);
            }

            if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.Q)
            {
#pragma warning disable CA1422
                request.AllowScanningByMediaScanner();
#pragma warning restore CA1422
            }
            else
            {
                request.SetDestinationInExternalPublicDir(
                    global::Android.OS.Environment.DirectoryDownloads,
                    filename);
            }

            request.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);

            var context = this.webView?.Context
                ?? global::Android.App.Application.Context;

            var downloadManager =
                (DownloadManager?)context.GetSystemService(Context.DownloadService);

            downloadManager?.Enqueue(request);

            Toast.MakeText(
                context,
                "Image download started...",
                ToastLength.Short)?.Show();
        }
    }
}
