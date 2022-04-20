using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Core.Views;
using WhereToFly.App.MapView;
using WhereToFly.Geo.Model;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Controls
{
    /// <summary>
    /// Web view showing a standalone HeightProfileView
    /// </summary>
    public class HeightProfileWebView : WebView
    {
        /// <summary>
        /// Task completion source that completes a task when the height profile view is
        /// initialized
        /// </summary>
        private readonly TaskCompletionSource<bool> taskCompletionSourceViewInitialized
            = new TaskCompletionSource<bool>();

        /// <summary>
        /// Height profile view to display
        /// </summary>
        private HeightProfileView heightProfileView;

        #region Binding properties
        /// <summary>
        /// Binding property for the source, specifying the track to display the height profile
        /// for
        /// </summary>
        public static readonly BindableProperty TrackProperty =
            BindableProperty.Create(
                propertyName: nameof(Track),
                returnType: typeof(Track),
                declaringType: typeof(HeightProfileWebView),
                defaultBindingMode: BindingMode.OneWay,
                defaultValue: null);
        #endregion

        #region View properties
        /// <summary>
        /// Track property
        /// </summary>
        public Track Track
        {
            get => (Track)this.GetValue(TrackProperty);
            set
            {
                this.SetValue(TrackProperty, value);
                Task.Run(async () => await this.SetTrackAsync(value));
            }
        }
        #endregion

        /// <summary>
        /// Creates a new height profile WebView object
        /// </summary>
        public HeightProfileWebView()
        {
            WebViewSource webViewSource =
                WebViewSourceFactory.Default.GetHeightProfileViewSource();

            this.Source = webViewSource;
            this.AutomationId = "HeightProfileWebView";

            this.Navigated += this.OnNavigated;
            this.SizeChanged += this.OnSizeChanged;
        }

        /// <summary>
        /// Called when the navigation to the web view has finished.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigated(object sender, WebNavigatedEventArgs args)
        {
            Debug.WriteLine($"OnNavigated: result={args.Result} event={args.NavigationEvent}");

            this.Navigated -= this.OnNavigated;

            bool useDarkTheme = Styles.ThemeHelper.CurrentTheme == Model.Theme.Dark;
            this.heightProfileView = new HeightProfileView(
                this,
                setBodyBackgroundColor: true,
                useDarkTheme);

            this.taskCompletionSourceViewInitialized.SetResult(true);

            if (this.Track != null)
            {
                this.heightProfileView.SetTrack(this.Track);

                if (this.Track.GroundHeightProfile.Any())
                {
                    this.heightProfileView.AddGroundProfile(this.Track.GroundHeightProfile);
                }
            }

            MainThread.BeginInvokeOnMainThread(async () => await this.ResizeViewHeight());
        }

        /// <summary>
        /// Called when the size of the view has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnSizeChanged(object sender, EventArgs args)
        {
            MainThread.BeginInvokeOnMainThread(async () => await this.ResizeViewHeight());
        }

        /// <summary>
        /// Resizes view height based on height of the chart container
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ResizeViewHeight()
        {
            string result = await this.EvaluateJavaScriptAsync(
                "javascript:document.getElementById('chartContainer').offsetHeight");

            Debug.WriteLine($"ResizeView: set height to {result}");

            if (double.TryParse(result, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out double height))
            {
                this.HeightRequest = height;
            }
        }

        /// <summary>
        /// Sets a new track to show in the height profile view
        /// </summary>
        /// <param name="track">track to show</param>
        /// <returns>task to wait on</returns>
        private async Task SetTrackAsync(Track track)
        {
            await this.taskCompletionSourceViewInitialized.Task;
            this.heightProfileView.SetTrack(track);

            if (track.GroundHeightProfile.Any())
            {
                this.heightProfileView.AddGroundProfile(track.GroundHeightProfile);
            }
        }
    }
}
