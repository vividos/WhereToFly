using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core.Views
{
    /// <summary>
    /// MapView control; the control is actually implemented as JavsScript code, but can be
    /// controlled using this class. JavaScript code is generated for function calls, and callback
    /// functions are called from JavaScript to C#.
    /// </summary>
    public class MapView
    {
        /// <summary>
        /// Web view where MapView control is used
        /// </summary>
        private readonly WebView webView;

        /// <summary>
        /// Current map overlay type
        /// </summary>
        private MapOverlayType mapOverlayType = MapOverlayType.OpenStreetMap;

        /// <summary>
        /// Current map shading mode
        /// </summary>
        private MapShadingMode mapShadingMode = MapShadingMode.Fixed10Am;

        /// <summary>
        /// Delegate of function to call when navigation to location should be started
        /// </summary>
        /// <param name="locationId">location id of location to navigate to</param>
        public delegate void OnNavigateToLocationCallback(string locationId);

        /// <summary>
        /// Event that is signaled when navigation to location should be started
        /// </summary>
        public event OnNavigateToLocationCallback NavigateToLocation;

        /// <summary>
        /// Delegate of function to call when location should be shared
        /// </summary>
        public delegate void OnShareMyLocationCallback();

        /// <summary>
        /// Event that is signaled when location should be shared
        /// </summary>
        public event OnShareMyLocationCallback ShareMyLocation;

        /// <summary>
        /// Gets or sets map overlay type
        /// </summary>
        public MapOverlayType MapOverlayType
        {
            get
            {
                return this.mapOverlayType;
            }

            set
            {
                if (this.mapOverlayType != value)
                {
                    this.mapOverlayType = value;

                    string js = string.Format("map.setOverlayType('{0}');", value);
                    this.RunJavaScript(js);
                }
            }
        }

        /// <summary>
        /// Gets or sets map shading mode
        /// </summary>
        public MapShadingMode MapShadingMode
        {
            get
            {
                return this.mapShadingMode;
            }

            set
            {
                if (this.mapShadingMode != value)
                {
                    this.mapShadingMode = value;

                    string js = string.Format("map.setShadingMode('{0}');", value);
                    this.RunJavaScript(js);
                }
            }
        }

        /// <summary>
        /// Creates a new MapView C# object
        /// </summary>
        /// <param name="webView">web view to use</param>
        public MapView(WebView webView)
        {
            this.webView = webView;

            this.webView.Navigating += this.OnNavigating_WebView;
        }

        /// <summary>
        /// Creates the MapView JavaScript object; this must be called before any other methods.
        /// </summary>
        /// <param name="initialCenterPoint">initial center point to be used for map view</param>
        /// <param name="initialZoomLevel">initial zoom level, in 2D zoom level steps</param>
        public void Create(MapPoint initialCenterPoint, int initialZoomLevel)
        {
            string initialRectangle = string.Format(
                "{{latitude:{0}, longitude:{1}}}",
                initialCenterPoint.Latitude.ToString(CultureInfo.InvariantCulture),
                initialCenterPoint.Longitude.ToString(CultureInfo.InvariantCulture));

            string js = string.Format(
                "map = new MapView({{id: 'mapElement', initialCenterPoint: {0}, initialZoomLevel: {1}}});",
                initialRectangle,
                initialZoomLevel);

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Zooms to given location
        /// </summary>
        /// <param name="position">position to zoom to</param>
        public void ZoomToLocation(MapPoint position)
        {
            string js = string.Format(
                "map.zoomToLocation({{latitude: {0}, longitude: {1}}});",
                position.Latitude.ToString(CultureInfo.InvariantCulture),
                position.Longitude.ToString(CultureInfo.InvariantCulture));

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Updates the "my location" pin in the map
        /// </summary>
        /// <param name="position">new position to use</param>
        /// <param name="zoomToLocation">indicates if view should also zoom to the location</param>
        public void UpdateMyLocation(MapPoint position, bool zoomToLocation)
        {
            string js = string.Format(
                "map.updateMyLocation({{latitude: {0}, longitude: {1}, zoomTo: {2}}});",
                position.Latitude.ToString(CultureInfo.InvariantCulture),
                position.Longitude.ToString(CultureInfo.InvariantCulture),
                zoomToLocation ? "true" : "false");

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Clears location list
        /// </summary>
        public void ClearLocationList()
        {
            this.RunJavaScript("map.clearLocationList();");
        }

        /// <summary>
        /// Adds a list of locations to the map, to be displayed as pins.
        /// </summary>
        /// <param name="locationList">list of locations to add</param>
        public void AddLocationList(List<Location> locationList)
        {
            var jsonLocationList =
                from location in locationList
                select new
                {
                    id = location.Id,
                    name = location.Name,
                    description = location.Description,
                    type = location.Type.ToString(),
                    latitude = location.MapLocation.Latitude,
                    longitude = location.MapLocation.Longitude,
                    elevation = (int)location.Elevation
                };

            string js = string.Format(
                "map.addLocationList({0});",
                JsonConvert.SerializeObject(jsonLocationList));

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Runs JavaScript code, in main thread
        /// </summary>
        /// <param name="js">javascript code snippet</param>
        private void RunJavaScript(string js)
        {
            Debug.WriteLine("run js: " + js);

            Device.BeginInvokeOnMainThread(() => this.webView.Eval(js));
        }

        /// <summary>
        /// Called when web view navigates to a new URL; used to bypass callback:// URLs.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigating_WebView(object sender, WebNavigatingEventArgs args)
        {
            if (args.Url.ToString().StartsWith("callback://"))
            {
                args.Cancel = true;

                string callbackParams = args.Url.ToString().Substring(11);

                int pos = callbackParams.IndexOf('/');
                Debug.Assert(pos > 0, "callback Uri must contain a slash after the function name");

                string functionName = callbackParams.Substring(0, pos);
                string jsonParameters = callbackParams.Substring(pos + 1);

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
                case "onNavigateToLocation":
                    this.NavigateToLocation?.Invoke(jsonParameters.Trim('\"'));

                    break;

                case "onShareMyLocation":
                    this.ShareMyLocation?.Invoke();

                    break;

                default:
                    Debug.Assert(false, "invalid callback function name");
                    break;
            }
        }
    }
}
