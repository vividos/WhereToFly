using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using WhereToFly.App.Logic;
using WhereToFly.App.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
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
        /// Current map imagery type
        /// </summary>
        private MapImageryType mapImageryType = MapImageryType.OpenStreetMap;

        /// <summary>
        /// Current map overlay type
        /// </summary>
        private MapOverlayType mapOverlayType = MapOverlayType.None;

        /// <summary>
        /// Current map shading mode
        /// </summary>
        private MapShadingMode mapShadingMode = MapShadingMode.Fixed10Am;

        /// <summary>
        /// Indicates if map control is already initialized
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// Maximum number of locations that are imported in one JavaScript call
        /// </summary>
        private const int MaxLocationListCount = 100;

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
        /// Delegate of function to call when find result should be added as waypoint
        /// </summary>
        public delegate void OnAddFindResultCallback(string name, MapPoint point);

        /// <summary>
        /// Event that is signaled when find result should be should be added as waypoint
        /// </summary>
        public event OnAddFindResultCallback AddFindResult;

        /// <summary>
        /// Delegate of function to call when long tap occured on map
        /// </summary>
        public delegate void OnLongTapCallback(MapPoint point);

        /// <summary>
        /// Event that is signaled when long tap occured on map
        /// </summary>
        public event OnLongTapCallback LongTap;

        /// <summary>
        /// Gets or sets map imagery type
        /// </summary>
        public MapImageryType MapImageryType
        {
            get
            {
                return this.mapImageryType;
            }

            set
            {
                if (this.mapImageryType != value)
                {
                    this.mapImageryType = value;

                    string js = string.Format("map.setMapImageryType('{0}');", value);
                    this.RunJavaScript(js);
                }
            }
        }

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

                    string js = string.Format("map.setMapOverlayType('{0}');", value);
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
        /// Coordinate display format to use for formatting coordinates
        /// </summary>
        public CoordinateDisplayFormat CoordinateDisplayFormat { get; set; }

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
            string initialCenterJs = string.Format(
                "{{latitude:{0}, longitude:{1}}}",
                initialCenterPoint.Latitude.ToString(CultureInfo.InvariantCulture),
                initialCenterPoint.Longitude.ToString(CultureInfo.InvariantCulture));

            string js = string.Format(
                "map = new MapView({{id: 'mapElement', initialCenterPoint: {0}, initialZoomLevel: {1}}});",
                initialCenterJs,
                initialZoomLevel);

            this.RunJavaScript(js);

            this.isInitialized = true;
        }

        /// <summary>
        /// Zooms to given location
        /// </summary>
        /// <param name="position">position to zoom to</param>
        public void ZoomToLocation(MapPoint position)
        {
            if (!this.isInitialized)
            {
                return;
            }

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
        /// <param name="altitudeInMeter">new altitide, in meter; when 0, it won't be used</param>
        /// <param name="timestamp">timestamp of location</param>
        /// <param name="zoomToLocation">indicates if view should also zoom to the location</param>
        public void UpdateMyLocation(MapPoint position, int altitudeInMeter, DateTimeOffset timestamp, bool zoomToLocation)
        {
            if (!this.isInitialized)
            {
                return;
            }

            var options = new
            {
                latitude = position.Latitude,
                longitude = position.Longitude,
                altitude = altitudeInMeter,
                timestamp,
                displayLatitude = DataFormatter.FormatLatLong(position.Latitude, this.CoordinateDisplayFormat),
                displayLongitude = DataFormatter.FormatLatLong(position.Longitude, this.CoordinateDisplayFormat),
                displayTimestamp = timestamp.ToLocalTime().ToString("yyyy-MM-dd HH\\:mm\\:ss"),
                zoomToLocation
            };

            string js = string.Format(
                "map.updateMyLocation({0});",
                JsonConvert.SerializeObject(options));

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
            if (locationList.Count > MaxLocationListCount)
            {
                this.ImportLargeLocationList(locationList);
                return;
            }

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
        /// Imports large location list
        /// </summary>
        /// <param name="locationList">large location list to import</param>
        private void ImportLargeLocationList(List<Location> locationList)
        {
            for (int i = 0; i < locationList.Count; i += MaxLocationListCount)
            {
                int remainingCount = Math.Min(MaxLocationListCount, locationList.Count - i);
                var locationSubList = locationList.GetRange(i, remainingCount);

                this.AddLocationList(locationSubList);
            }
        }

        /// <summary>
        /// Shows the find result pin and zooms to it
        /// </summary>
        /// <param name="text">text of find result</param>
        /// <param name="point">find result map point</param>
        public void ShowFindResult(string text, MapPoint point)
        {
            var options = new
            {
                name = text,
                latitude = point.Latitude,
                longitude = point.Longitude,
                displayLatitude = DataFormatter.FormatLatLong(point.Latitude, this.CoordinateDisplayFormat),
                displayLongitude = DataFormatter.FormatLatLong(point.Longitude, this.CoordinateDisplayFormat)
            };

            string js = string.Format(
                "map.showFindResult({0});",
                JsonConvert.SerializeObject(options));

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

                case "onAddFindResult":
                    var parameters = JsonConvert.DeserializeObject<AddFindResultParameter>(jsonParameters);
                    var point = new MapPoint(parameters.Latitude, parameters.Longitude);
                    this.AddFindResult?.Invoke(parameters.Name, point);
                    break;

                case "onLongTap":
                    var longTapParameters = JsonConvert.DeserializeObject<LongTapParameter>(jsonParameters);
                    var longTapPoint = new MapPoint(longTapParameters.Latitude, longTapParameters.Longitude);
                    this.LongTap?.Invoke(longTapPoint);
                    break;

                default:
                    Debug.Assert(false, "invalid callback function name");
                    break;
            }
        }

        /// <summary>
        /// Parameter for AddFindResult JavaScript event
        /// </summary>
        private class AddFindResultParameter
        {
            /// <summary>
            /// Name of find result to add
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Latitude of map point to add
            /// </summary>
            public double Latitude { get; set; }

            /// <summary>
            /// Longitude of map point to add
            /// </summary>
            public double Longitude { get; set; }
        }

        /// <summary>
        /// Parameter for OnLongTap JavaScript event
        /// </summary>
        private class LongTapParameter
        {
            /// <summary>
            /// Latitude of map point where long tap occured
            /// </summary>
            public double Latitude { get; set; }

            /// <summary>
            /// Longitude of map point where long tap occured
            /// </summary>
            public double Longitude { get; set; }
        }
    }
}
