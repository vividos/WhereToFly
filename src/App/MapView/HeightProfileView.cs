using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WhereToFly.Geo.Model;
using Xamarin.Forms;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// View that shows a track height profile
    /// </summary>
    public class HeightProfileView
    {
        /// <summary>
        /// Web view where HeightProfileView control is used
        /// </summary>
        private readonly WebView webView;

        /// <summary>
        /// Delegate of function to call when user hovers over or clicks on a height profile point
        /// </summary>
        /// <param name="trackPointIndex">track point index where click occured over</param>
        public delegate void OnHoverOrClickCallback(int trackPointIndex);

        /// <summary>
        /// Event that is signaled when user hovers over a height profile point
        /// </summary>
        public event OnHoverOrClickCallback Hover;

        /// <summary>
        /// Event that is signaled when user clicks on a height profile point
        /// </summary>
        public event OnHoverOrClickCallback Click;

        /// <summary>
        /// Creates a new HeightProfileView C# object
        /// </summary>
        /// <param name="webView">web view to use</param>
        /// <param name="setBodyBackgroundColor">indicates if body background should be set</param>
        /// <param name="useDarkTheme">indicates if a dark theme should be used for the chart</param>
        public HeightProfileView(WebView webView, bool setBodyBackgroundColor, bool useDarkTheme)
        {
            this.webView = webView;

            var callbackHandler = new WebViewCallbackSchemaHandler(webView);
            callbackHandler.RegisterHandler(
                "onClick",
                (jsonParameters) => this.Click?.Invoke(int.Parse(jsonParameters)));

            callbackHandler.RegisterHandler(
                "onHover",
                (jsonParameters) => this.Hover?.Invoke(int.Parse(jsonParameters)));

            var options = new
            {
                id = "chartElement",
                containerId = "chartContainer",
                setBodyBackgroundColor,
                useDarkTheme
            };

            string jsonOptions = JsonConvert.SerializeObject(options);
            string js = $"heightProfileView = new WhereToFly.heightProfileView.HeightProfileView({jsonOptions});";
            this.RunJavaScript(js);
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
                    x.Altitude ?? 0.0
                });

            List<double> timePointsList = null;

            var firstTrackPoint = track.TrackPoints.FirstOrDefault();
            if (firstTrackPoint != null &&
                firstTrackPoint.Time.HasValue)
            {
                timePointsList = track.TrackPoints.Select(x => x.Time.Value.ToUnixTimeMilliseconds() / 1000.0).ToList();
            }

            var trackJsonObject = new
            {
                id = track.Id,
                listOfTrackPoints = trackPointsList,
                listOfTimePoints = timePointsList,
            };

            string js = $"heightProfileView.setTrack({JsonConvert.SerializeObject(trackJsonObject)});";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Adds a ground profile using given elevations. The number of elevation values must
        /// match the number of track points from the SetTrack() call.
        /// </summary>
        /// <param name="elevationValues">elevation values</param>
        public void AddGroundProfile(IEnumerable<double> elevationValues)
        {
            string elevations = JsonConvert.SerializeObject(elevationValues);
            string js = $"heightProfileView.addGroundProfile({elevations});";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Runs JavaScript code, in main thread
        /// </summary>
        /// <param name="js">javascript code snippet</param>
        private void RunJavaScript(string js)
        {
            Debug.WriteLine("run js: " + js.Substring(0, Math.Min(80, js.Length)));

            Device.BeginInvokeOnMainThread(() => this.webView.Eval(js));
        }
    }
}
