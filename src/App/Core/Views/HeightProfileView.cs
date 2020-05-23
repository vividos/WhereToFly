using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WhereToFly.App.Geo;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
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
        public HeightProfileView(WebView webView)
        {
            this.webView = webView;

            this.webView.Navigating += this.OnNavigating_WebView;

            string js = "heightProfileView = new HeightProfileView({id: 'chartElement'});";
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
        public void AddGroundProfile(double[] elevationValues)
        {
            string elevations = JsonConvert.SerializeObject(elevationValues);
            string js = $"heightProfileView.addGroundProfile({elevations});";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Called when web view navigates to a new URL; used to bypass callback:// URLs.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigating_WebView(object sender, WebNavigatingEventArgs args)
        {
            const string CallbackSchema = "callback://";
            if (args.Url.ToString().StartsWith(CallbackSchema))
            {
                args.Cancel = true;

                string callbackParams = args.Url.ToString().Substring(CallbackSchema.Length);

                int pos = callbackParams.IndexOf('/');
                Debug.Assert(pos > 0, "callback Uri must contain a slash after the function name");

                string functionName = callbackParams.Substring(0, pos);
                string jsonParameters = callbackParams.Substring(pos + 1);

                jsonParameters = System.Net.WebUtility.UrlDecode(jsonParameters);

                this.ExecuteCallback(functionName, jsonParameters);
            }
        }

        /// <summary>
        /// Executes callback function
        /// </summary>
        /// <param name="functionName">function name of function to execute</param>
        /// <param name="jsonParameters">JSON formatted parameters for function</param>
        private void ExecuteCallback(string functionName, string jsonParameters)
        {
            switch (functionName)
            {
                case "onClick":
                    this.Click?.Invoke(int.Parse(jsonParameters));
                    break;

                case "onHover":
                    this.Hover?.Invoke(int.Parse(jsonParameters));
                    break;

                default:
                    Debug.Assert(false, "invalid callback function name");
                    break;
            }
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
