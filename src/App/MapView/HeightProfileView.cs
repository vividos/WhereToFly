using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Web view showing a track height profile
    /// </summary>
    public class HeightProfileView : WebView
    {
        /// <summary>
        /// Task completion source that completes a task when the height profile view is
        /// initialized
        /// </summary>
        private readonly TaskCompletionSource<bool> taskCompletionSourceViewInitialized
            = new();

        /// <summary>
        /// Delegate of function to call when user hovers over or clicks on a height profile point
        /// </summary>
        /// <param name="trackPointIndex">track point index where click occured over</param>
        public delegate void OnHoverOrClickCallback(int trackPointIndex);

        /// <summary>
        /// Event that is signaled when user hovers over a height profile point
        /// </summary>
        public event OnHoverOrClickCallback? Hover;

        /// <summary>
        /// Event that is signaled when user clicks on a height profile point
        /// </summary>
        public event OnHoverOrClickCallback? Click;

        #region Binding properties
        /// <summary>
        /// Binding property for the track source, specifying the track to display the height
        /// profile for
        /// </summary>
        public static readonly BindableProperty TrackProperty =
            BindableProperty.Create(
                propertyName: nameof(Track),
                returnType: typeof(Track),
                declaringType: typeof(HeightProfileView),
                defaultBindingMode: BindingMode.OneWay,
                defaultValue: null);

        /// <summary>
        /// Binding property for the flag UseDarkTheme
        /// </summary>
        public static readonly BindableProperty UseDarkThemeProperty =
            BindableProperty.Create(
                propertyName: nameof(UseDarkTheme),
                returnType: typeof(bool),
                declaringType: typeof(HeightProfileView),
                defaultBindingMode: BindingMode.OneWay,
                defaultValue: false);
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

        /// <summary>
        /// Indicates if a dark theme should be used to display height profile
        /// </summary>
        public bool UseDarkTheme
        {
            get => (bool)this.GetValue(UseDarkThemeProperty);
            set => this.SetValue(UseDarkThemeProperty, value);
        }
        #endregion

        /// <summary>
        /// Creates a new height profile WebView object
        /// </summary>
        public HeightProfileView()
        {
            this.Source = WebViewSourceFactory.GetHeightProfileViewSource();

            this.RegisterWebViewCallbacks();

            this.AutomationId = "HeightProfileView";

            this.Navigated += this.OnNavigated;
            this.SizeChanged += this.OnSizeChanged;
        }

        /// <summary>
        /// Registers callback functions from JavaScript to C#
        /// </summary>
        private void RegisterWebViewCallbacks()
        {
            var callbackHandler = new WebViewCallbackSchemaHandler(this);
            callbackHandler.RegisterHandler(
                "onClick",
                (jsonParameters) => this.Click?.Invoke(int.Parse(jsonParameters)));

            callbackHandler.RegisterHandler(
                "onHover",
                (jsonParameters) => this.Hover?.Invoke(int.Parse(jsonParameters)));
        }

        /// <summary>
        /// Called when the navigation to the web view has finished.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigated(object? sender, WebNavigatedEventArgs args)
        {
            Debug.WriteLine($"OnNavigated: result={args.Result} event={args.NavigationEvent}");

            this.Navigated -= this.OnNavigated;

            var options = new
            {
                id = "heightProfileView",
                trackName = string.Empty,
                setBodyBackgroundColor = true,
                useDarkTheme = this.UseDarkTheme,
            };

            string jsonOptions = ConvertAnonymousObjectToJson(options);
            string js = $"heightProfileView = new WhereToFly.heightProfileView.HeightProfileView({jsonOptions});";
            this.RunJavaScript(js);

            this.taskCompletionSourceViewInitialized.SetResult(true);

            if (this.Track != null)
            {
                this.SetTrack(this.Track);

                if (this.Track.GroundHeightProfile.Count != 0)
                {
                    this.AddGroundProfile(this.Track.GroundHeightProfile);
                }

                this.UpdateView();
            }

            MainThread.BeginInvokeOnMainThread(async () => await this.ResizeViewHeight());
        }

        /// <summary>
        /// Called when the size of the view has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnSizeChanged(object? sender, EventArgs args)
        {
            MainThread.BeginInvokeOnMainThread(
                async () => await this.ResizeViewHeight());
        }

        /// <summary>
        /// Resizes view height based on height of the chart container
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ResizeViewHeight()
        {
            await this.taskCompletionSourceViewInitialized.Task;

            string result = await this.EvaluateJavaScriptAsync(
                "javascript:document.getElementById('heightProfileView').offsetHeight");

            Debug.WriteLine($"ResizeView: set height to {result}");

            if (double.TryParse(
                result,
                NumberStyles.Float,
                NumberFormatInfo.InvariantInfo,
                out double height))
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
            this.SetTrack(track);

            if (track.GroundHeightProfile.Count != 0)
            {
                this.AddGroundProfile(track.GroundHeightProfile);
            }

            this.UpdateView();
        }

        /// <summary>
        /// Sets track to display height profile
        /// </summary>
        /// <param name="track">track to use</param>
        public void SetTrack(Track track)
        {
            var trackPointsList =
                track.TrackPoints.SelectMany(x => new double[]
                {
                    x.Longitude,
                    x.Latitude,
                    x.Altitude ?? 0.0,
                });

            List<double>? timePointsList = null;

            var firstTrackPoint = track.TrackPoints.FirstOrDefault();
            if (firstTrackPoint != null &&
                firstTrackPoint.Time.HasValue)
            {
                timePointsList = track.TrackPoints
                    .Select(x => x.Time?.ToUnixTimeMilliseconds() / 1000.0 ?? 0.0)
                    .ToList();
            }

            var trackJsonObject = new
            {
                id = track.Id,
                listOfTrackPoints = trackPointsList,
                listOfTimePoints = timePointsList,
            };

            string json = ConvertAnonymousObjectToJson(trackJsonObject);
            string js = $"heightProfileView.setTrack({json});";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Adds a ground profile using given elevations. The number of elevation values must
        /// match the number of track points from the SetTrack() call.
        /// </summary>
        /// <param name="elevationValues">elevation values</param>
        public void AddGroundProfile(IEnumerable<double> elevationValues)
        {
            string elevations = ConvertAnonymousObjectToJson(elevationValues);
            string js = $"heightProfileView.addGroundProfile({elevations});";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Updates height profile view; this must be called after calling one or more of the
        /// following methods: <see cref="HeightProfileView.SetTrack"/>,
        /// <see cref="HeightProfileView.AddGroundProfile"/>.
        /// </summary>
        public void UpdateView()
        {
            this.RunJavaScript("heightProfileView.updateView();");
        }

        /// <summary>
        /// Runs JavaScript code, in main thread
        /// </summary>
        /// <param name="js">javascript code snippet</param>
        private void RunJavaScript(string js)
        {
            Debug.WriteLine("run js: " + js.Substring(0, Math.Min(80, js.Length)));

            this.Dispatcher.DispatchAsync(() => this.Eval(js));
        }

        /// <summary>
        /// Converts an anonymous object to JSON; the object must only contain simple types such
        /// as string, bool, int or double, or else the conversion will fail (when running with
        /// AOT in release mode)
        /// </summary>
        /// <param name="obj">object to convert</param>
        /// <returns>JSON text</returns>
        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
            Justification = "The method is only passed anonymous objects with simple types")]
        [UnconditionalSuppressMessage(
            "AOT",
            "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
            Justification = "The method is only passed anonymous objects with simple types")]
        private static string ConvertAnonymousObjectToJson(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
