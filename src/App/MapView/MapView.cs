using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

// make MapView internals visible to unit tests
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WhereToFly.App.UnitTest")]

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// MapView control; the control is actually implemented as JavsScript code, but can be
    /// controlled using this class. JavaScript code is generated for function calls, and callback
    /// functions are called from JavaScript to C#.
    /// </summary>
    public class MapView : WebView, IMapView, ITerrainHeightProvider
    {
        /// <summary>
        /// Maximum number of locations that are imported in one JavaScript call
        /// </summary>
        private const int MaxLocationListCount = 100;

        /// <summary>
        /// Task completion source for when the web page has been loaded
        /// </summary>
        private readonly TaskCompletionSource<bool> taskCompletionSourcePageLoaded
            = new();

        /// <summary>
        /// List of nearby POIs already added to the map
        /// </summary>
        private readonly List<string> nearbyPoiIdList = new();

        /// <summary>
        /// Task completion source for when map is fully initialized
        /// </summary>
        private TaskCompletionSource<bool> taskCompletionSourceMapInitialized
            = new();

        /// <summary>
        /// Task completion source for when SampleTrackHeights() has finished
        /// </summary>
        private TaskCompletionSource<double[]> taskCompletionSourceSampleTrackHeights;

        /// <summary>
        /// Task completion source for when ExportLayer() has finished
        /// </summary>
        private TaskCompletionSource<byte[]> taskCompletionSourceExportLayer;

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
        private MapShadingMode mapShadingMode = MapShadingMode.CurrentTime;

        /// <summary>
        /// Current value for "entity clustering" flag
        /// </summary>
        private bool useEntityClustering = true;

        /// <summary>
        /// Last set "my location" position
        /// </summary>
        private MapPoint lastMyLocation;

        /// <summary>
        /// Last used compass target title
        /// </summary>
        private string lastCompassTargetTitle;

        /// <summary>
        /// Last used compass target location
        /// </summary>
        private MapPoint lastCompassTargetLocation;

        /// <summary>
        /// Last used compass target direction
        /// </summary>
        private int? lastCompassTargetDirection;

        /// <summary>
        /// Indicates if map control is already initialized
        /// </summary>
        private bool IsInitialized => this.taskCompletionSourceMapInitialized.Task.IsCompleted;

        /// <summary>
        /// Task that is in "completed" state when map is initialized. This is the case when
        /// CreateAsync() created the JavaScript MapView object and its ctor ran to completion.
        /// </summary>
        public Task MapInitializedTask => this.taskCompletionSourceMapInitialized.Task;

        /// <summary>
        /// Service for finding nearby POIs
        /// </summary>
        public INearbyPoiService NearbyPoiService { get; set; }

        /// <summary>
        /// Delegate of function to call when location details should be shown
        /// </summary>
        /// <param name="locationId">location id of location to navigate to</param>
        public delegate void OnShowLocationDetailsCallback(string locationId);

        /// <summary>
        /// Event that is signaled when location details should be shown
        /// </summary>
        public event OnShowLocationDetailsCallback ShowLocationDetails;

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
        /// <param name="name">find result name</param>
        /// <param name="point">map point</param>
        public delegate void OnAddFindResultCallback(string name, MapPoint point);

        /// <summary>
        /// Event that is signaled when find result should be should be added as waypoint
        /// </summary>
        public event OnAddFindResultCallback AddFindResult;

        /// <summary>
        /// Delegate of function to call when long tap occured on map
        /// </summary>
        /// <param name="point">map point of long tap</param>
        public delegate void OnLongTapCallback(MapPoint point);

        /// <summary>
        /// Event that is signaled when long tap occured on map
        /// </summary>
        public event OnLongTapCallback LongTap;

        /// <summary>
        /// Delegate of function to call when adding a location to tour planning
        /// </summary>
        /// <param name="locationId">location id of location to add</param>
        public delegate void OnAddTourPlanLocationCallback(string locationId);

        /// <summary>
        /// Event that is signaled when adding a location to tour planning
        /// </summary>
        public event OnAddTourPlanLocationCallback AddTourPlanLocation;

        /// <summary>
        /// Delegate of function to call when last shown location should be updated
        /// </summary>
        /// <param name="point">map point of last shown location</param>
        /// <param name="viewingDistance">current viewing distance</param>
        public delegate void OnUpdateLastShownLocationCallback(MapPoint point, int viewingDistance);

        /// <summary>
        /// Event that is signaled when last shown location should be updated
        /// </summary>
        public event OnUpdateLastShownLocationCallback UpdateLastShownLocation;

        /// <summary>
        /// Delegate of function to call when user sets a location as the compass target location
        /// </summary>
        /// <param name="locationId">location ID of new target location; may be null</param>
        public delegate void OnSetLocationAsCompassTargetCallback(string locationId);

        /// <summary>
        /// Event that is signaled when user sets a location as compass target
        /// </summary>
        public event OnSetLocationAsCompassTargetCallback SetLocationAsCompassTarget;

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

                    string js = string.Format("await map.setMapImageryType('{0}');", value);
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

                    string js = string.Format("await map.setMapOverlayType('{0}');", value);
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
        /// Gets or sets if entity clustering is enabled
        /// </summary>
        public bool UseEntityClustering
        {
            get
            {
                return this.useEntityClustering;
            }

            set
            {
                if (this.useEntityClustering != value)
                {
                    this.useEntityClustering = value;

                    string js = string.Format(
                        "map.setEntityClustering({0});",
                        value.ToString().ToLowerInvariant());
                    this.RunJavaScript(js);
                }
            }
        }

        /// <summary>
        /// Action to log errors
        /// </summary>
        public Action<Exception> LogErrorAction { get; set; }

        /// <summary>
        /// Creates a new MapView C# object
        /// </summary>
        public MapView()
        {
            Task.Run(this.InitSourceAsync);

            this.RegisterWebViewCallbacks();
        }

        /// <summary>
        /// Initializes web view source
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task InitSourceAsync()
        {
            this.Navigated += this.OnNavigated;

            this.Source =
                await WebViewSourceFactory.Instance.GetMapViewSource();
            this.OnPropertyChanged(nameof(this.Source));
        }

        /// <summary>
        /// Called when navigation to the web page has finished
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigated(object sender, WebNavigatedEventArgs args)
        {
            if ((args.Url.Contains("/mapView.html") || args.Url.Contains("android_asset")) &&
                args.Result == WebNavigationResult.Success)
            {
                this.Navigated -= this.OnNavigated;

                this.taskCompletionSourcePageLoaded.SetResult(true);
            }
        }

        /// <summary>
        /// Creates the MapView JavaScript object; this must be called before any other methods.
        /// </summary>
        /// <param name="initialCenterPoint">initial center point to be used for map view</param>
        /// <param name="initialViewingDistance">initial viewing distance, in meters from terrain</param>
        /// <param name="useEntityClustering">indicates if entity clustering should be used</param>
        /// <returns>task to wait on</returns>
        public async Task CreateAsync(MapPoint initialCenterPoint, int initialViewingDistance, bool useEntityClustering)
        {
            await this.taskCompletionSourcePageLoaded.Task;

            if (this.IsInitialized)
            {
                this.taskCompletionSourceMapInitialized = new TaskCompletionSource<bool>();
            }

            var options = new
            {
                id = "mapElement",
                messageBandId = "messageBand",
                liveTrackToolbarId = "liveTrackToolbar",
                initialCenterPoint = new
                {
                    latitude = initialCenterPoint.Latitude,
                    longitude = initialCenterPoint.Longitude,
                },
                initialViewingDistance,
                hasMouse = Device.RuntimePlatform == Device.UWP,
                useAsynchronousPrimitives = true,
                useEntityClustering,
            };

            string js = string.Format(
                "map = new WhereToFly.mapView.MapView({0});",
                JsonConvert.SerializeObject(options));

            this.RunJavaScript(js);

            await this.MapInitializedTask;
        }

        /// <summary>
        /// Shows a message band at the top of the map, with given message text
        /// </summary>
        /// <param name="messageText">message text</param>
        public void ShowMessageBand(string messageText)
        {
            string js = $"map.showMessageBand(\"{messageText}\");";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Hides message band again
        /// </summary>
        public void HideMessageBand()
        {
            this.RunJavaScript("map.hideMessageBand();");
        }

        /// <summary>
        /// Zooms to given location
        /// </summary>
        /// <param name="position">position to zoom to</param>
        public void ZoomToLocation(MapPoint position)
        {
            if (!this.IsInitialized)
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
        /// Zooms to rectangle specified by two map points
        /// </summary>
        /// <param name="minPosition">minimum position values</param>
        /// <param name="maxPosition">maximum position values</param>
        public void ZoomToRectangle(MapPoint minPosition, MapPoint maxPosition)
        {
            if (!this.IsInitialized)
            {
                return;
            }

            var rectangle = new
            {
                minLatitude = minPosition.Latitude,
                maxLatitude = maxPosition.Latitude,
                minLongitude = minPosition.Longitude,
                maxLongitude = maxPosition.Longitude,
                minAltitude = minPosition.Altitude ?? 0.0,
                maxAltitude = maxPosition.Altitude ?? 0.0,
            };

            string js = string.Format(
                "map.zoomToRectangle({0});",
                JsonConvert.SerializeObject(rectangle));

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Updates the "my location" pin in the map
        /// </summary>
        /// <param name="position">new position to use</param>
        /// <param name="positionAccuracyInMeter">position accuracy, in meter</param>
        /// <param name="speedInKmh">current speed, in km/h</param>
        /// <param name="timestamp">timestamp of location</param>
        /// <param name="zoomToLocation">indicates if view should also zoom to the location</param>
        public void UpdateMyLocation(
            MapPoint position,
            int positionAccuracyInMeter,
            double speedInKmh,
            DateTimeOffset timestamp,
            bool zoomToLocation)
        {
            this.lastMyLocation = position;

            if (!this.IsInitialized)
            {
                return;
            }

            var options = new
            {
                latitude = position.Latitude,
                longitude = position.Longitude,
                positionAccuracy = positionAccuracyInMeter,
                positionAccuracyColor = ColorFromPositionAccuracy(positionAccuracyInMeter),
                altitude = position.Altitude.GetValueOrDefault(0.0),
                speed = speedInKmh,
                timestamp,
                displayLatitude = GeoDataFormatter.FormatLatLong(position.Latitude, this.CoordinateDisplayFormat),
                displayLongitude = GeoDataFormatter.FormatLatLong(position.Longitude, this.CoordinateDisplayFormat),
                displayTimestamp = timestamp.ToLocalTime().ToString("yyyy-MM-dd HH\\:mm\\:ss"),
                displaySpeed = string.Format("{0:F1} km/h", speedInKmh),
                zoomToLocation,
            };

            string js = string.Format(
                "map.updateMyLocation({0});",
                JsonConvert.SerializeObject(options));

            this.RunJavaScript(js);

            // also update compass target, if previously set
            if (this.lastCompassTargetLocation != null)
            {
                this.SetCompassTarget(
                    this.lastCompassTargetTitle,
                    this.lastCompassTargetLocation,
                    zoomToPolyline: false);
            }
            else if (this.lastCompassTargetDirection != null)
            {
                this.SetCompassDirection(
                    this.lastCompassTargetTitle,
                    this.lastCompassTargetDirection.Value);
            }
        }

        /// <summary>
        /// Sets a compass target, displaying a line from the current location to the target
        /// location. The line is shown as soon as the "my location" is known to the map.
        /// </summary>
        /// <param name="title">compass target title</param>
        /// <param name="position">compass target position</param>
        /// <param name="zoomToPolyline">
        /// indicates if compass target polyline should be zoomed to
        /// </param>
        public void SetCompassTarget(string title, MapPoint position, bool zoomToPolyline)
        {
            this.lastCompassTargetTitle = title;
            this.lastCompassTargetLocation = position;
            this.lastCompassTargetDirection = null;

            double? distanceInKm = null;
            double? heightDifferenceInMeter = null;
            double? directionAngle = null;

            if (this.lastMyLocation != null)
            {
                distanceInKm = this.lastMyLocation.DistanceTo(this.lastCompassTargetLocation) / 1000.0;

                if (this.lastMyLocation.Altitude.HasValue &&
                    this.lastCompassTargetLocation.Altitude.HasValue)
                {
                    heightDifferenceInMeter =
                        this.lastCompassTargetLocation.Altitude.Value -
                        this.lastMyLocation.Altitude.Value;
                }

                directionAngle = this.lastMyLocation.CourseTo(this.lastCompassTargetLocation);
            }

            var options = new
            {
                title,
                latitude = position.Latitude,
                longitude = position.Longitude,
                altitude = position.Altitude.GetValueOrDefault(0.0),
                displayLatitude = GeoDataFormatter.FormatLatLong(position.Latitude, this.CoordinateDisplayFormat),
                displayLongitude = GeoDataFormatter.FormatLatLong(position.Longitude, this.CoordinateDisplayFormat),
                distanceInKm,
                heightDifferenceInMeter,
                directionAngle,
                zoomToPolyline,
                hideTargetLocation = false,
            };

            string js = $"await map.setCompassTarget({JsonConvert.SerializeObject(options)});";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Sets a compass direction, displaying a line from the current location in a specific
        /// direction. The line is shown as soon as the "my location" is known to the map.
        /// </summary>
        /// <param name="title">compass target title</param>
        /// <param name="directionAngleInDegrees">direction angle, in degrees</param>
        public void SetCompassDirection(string title, int directionAngleInDegrees)
        {
            this.lastCompassTargetTitle = title;
            this.lastCompassTargetLocation = null;
            this.lastCompassTargetDirection = directionAngleInDegrees;

            if (this.lastMyLocation == null)
            {
                return;
            }

            var position = this.lastMyLocation.PolarOffset(
                50.0 * 1000.0,
                directionAngleInDegrees,
                0.0);

            double? distanceInKm = null;
            double? heightDifferenceInMeter = null;

            var options = new
            {
                title,
                latitude = position.Latitude,
                longitude = position.Longitude,
                distanceInKm,
                heightDifferenceInMeter,
                directionAngle = directionAngleInDegrees,
                zoomToPolyline = false,
                hideTargetLocation = false,
            };

            string js = $"await map.setCompassTarget({JsonConvert.SerializeObject(options)});";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Clears compass target or direction, hiding the line from the current location.
        /// </summary>
        public void ClearCompass()
        {
            this.lastCompassTargetTitle = null;
            this.lastCompassTargetLocation = null;
            this.lastCompassTargetDirection = null;

            this.RunJavaScript("map.clearCompass();");
        }

        /// <summary>
        /// Finds nearby POIs in the currently shown camera view rectangle.
        /// </summary>
        /// <returns>task to wait on</returns>
        /// <exception cref="Exception">
        /// thrown when nearby POIs couldn't be retrieved
        /// </exception>
        public async Task FindNearbyPois()
        {
            Debug.WriteLine(
                this.NearbyPoiService != null,
                "must set a NearbyPoiService before calling");

            if (this.NearbyPoiService == null)
            {
                throw new InvalidOperationException("no NearbyPoiService set");
            }

            try
            {
                this.ShowMessageBand("Loading nearby POIs...");

                MapRectangle area = await this.GetViewRectangle();

                // limit size of area
                const double NearbyPoisMaxRequestLatitudeLongitude = 2.0;

                var areaCenter = area.Center;
                if (area.Width > NearbyPoisMaxRequestLatitudeLongitude)
                {
                    area.West = areaCenter.Longitude - (NearbyPoisMaxRequestLatitudeLongitude / 2.0);
                    area.East = areaCenter.Longitude + (NearbyPoisMaxRequestLatitudeLongitude / 2.0);
                }

                if (area.Height > NearbyPoisMaxRequestLatitudeLongitude)
                {
                    area.South = areaCenter.Latitude - (NearbyPoisMaxRequestLatitudeLongitude / 2.0);
                    area.North = areaCenter.Latitude + (NearbyPoisMaxRequestLatitudeLongitude / 2.0);
                }

                IEnumerable<Location> newLocations =
                    await this.NearbyPoiService.Get(area, this.nearbyPoiIdList);

                this.AddNearbyPoiLocations(newLocations);
            }
            finally
            {
                this.HideMessageBand();
            }
        }

        /// <summary>
        /// Returns the current view rectangle
        /// </summary>
        /// <returns>map rectangle of current view</returns>
        private async Task<MapRectangle> GetViewRectangle()
        {
            string js = "map.getViewRectangle();";
            string result = await Device.InvokeOnMainThreadAsync(
                () => this.EvaluateJavaScriptAsync(js));

            result = result
                .Replace("\\\\", "\\")
                .Replace("\\\"", "\"");

            return JsonConvert.DeserializeObject<MapRectangle>(result);
        }

        /// <summary>
        /// Adds a list of nearby POI locations
        /// </summary>
        /// <param name="nearbyPoiLocations">list of nearby POI locations</param>
        public void AddNearbyPoiLocations(IEnumerable<Location> nearbyPoiLocations)
        {
            this.nearbyPoiIdList.AddRange(
                nearbyPoiLocations.Select(location => location.Id));

            var jsonLocationList =
                from location in nearbyPoiLocations
                select CreateJsonObjectFromLocation(location);

            string js = string.Format(
                "map.addNearbyPoiLocations({0});",
                JsonConvert.SerializeObject(jsonLocationList));

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Returns an HTML color from a position accuracy value.
        /// </summary>
        /// <param name="positionAccuracyInMeter">position accuracy, in meter</param>
        /// <returns>HTML color in format #rrggbb</returns>
        private static string ColorFromPositionAccuracy(int positionAccuracyInMeter)
        {
            if (positionAccuracyInMeter < 40)
            {
                return "#00c000"; // green
            }
            else if (positionAccuracyInMeter < 120)
            {
                return "#e0e000"; // yellow
            }
            else if (positionAccuracyInMeter < 200)
            {
                return "#ff8000"; // orange
            }
            else
            {
                return "#c00000"; // red
            }
        }

        /// <summary>
        /// Adds layer to map
        /// </summary>
        /// <param name="layer">layer to add</param>
        /// <returns>task to wait on</returns>
        public async Task AddLayer(Layer layer)
        {
            var layerObject = new
            {
                id = layer.Id,
                name = layer.Name,
                type = layer.LayerType.ToString(),
                isVisible = layer.IsVisible,
                data = layer.Data,
            };

            string js = string.Format(
                "await map.addLayer({0});",
                JsonConvert.SerializeObject(layerObject));

            await this.RunJavaScriptAsync(js);
        }

        /// <summary>
        /// Zooms to given layer on map
        /// </summary>
        /// <param name="layer">layer to zoom to</param>
        public void ZoomToLayer(Layer layer)
        {
            string js = $"map.zoomToLayer(\"{layer.Id}\");";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Sets new visibility of given layer; IsVisible property of layer is used.
        /// </summary>
        /// <param name="layer">layer to set visibility</param>
        public void SetLayerVisibility(Layer layer)
        {
            var options = new
            {
                id = layer.Id,
                isVisible = layer.IsVisible,
            };

            string js = string.Format(
                "map.setLayerVisibility({0});",
                JsonConvert.SerializeObject(options));

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Removes layer; built-in layers can't be removed.
        /// </summary>
        /// <param name="layer">layer to remove</param>
        public void RemoveLayer(Layer layer)
        {
            string js = $"map.removeLayer(\"{layer.Id}\");";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Clears layer list; built-in layers can't be removed.
        /// </summary>
        public void ClearLayerList()
        {
            this.RunJavaScript("map.clearLayerList();");
        }

        /// <summary>
        /// Exports given CZML layer as KMZ bytestream
        /// </summary>
        /// <param name="layer">layer to export</param>
        /// <returns>exported KMZ byte stream</returns>
        public async Task<byte[]> ExportLayerAsync(Layer layer)
        {
            this.taskCompletionSourceExportLayer = new TaskCompletionSource<byte[]>();

            this.RunJavaScript($"map.exportLayer(\"{layer.Id}\");");

            return await this.taskCompletionSourceExportLayer.Task;
        }

        /// <summary>
        /// Clears location list
        /// </summary>
        public void ClearLocationList()
        {
            this.RunJavaScript("map.clearLocationList();");
        }

        /// <summary>
        /// Adds a single locations to the map, to be displayed as pin.
        /// </summary>
        /// <param name="location">location to add</param>
        public void AddLocation(Location location)
        {
            object jsonLocation = CreateJsonObjectFromLocation(location);

            string js = string.Format(
                "await map.addLocationList([{0}]);",
                JsonConvert.SerializeObject(jsonLocation));

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Creates a C# object suitable for conversion to JSON, in order to use in a JavaScript
        /// call.
        /// </summary>
        /// <param name="location">location to convert</param>
        /// <returns>C# object</returns>
        private static object CreateJsonObjectFromLocation(Location location)
        {
            return new
            {
                id = location.Id,
                name = location.Name,
                description = location.Description,
                type = location.Type.ToString(),
                latitude = location.MapLocation.Latitude,
                longitude = location.MapLocation.Longitude,
                altitude = location.MapLocation.Altitude.GetValueOrDefault(0.0),
                takeoffDirections = (int)location.TakeoffDirections,
                isPlanTourLocation = location.IsPlanTourLocation,
                properties = location.Properties.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
            };
        }

        /// <summary>
        /// Adds a list of locations to the map, to be displayed as pins.
        /// </summary>
        /// <param name="locationList">list of locations to add</param>
        /// <returns>task to wait on</returns>
        public async Task AddLocationList(List<Location> locationList)
        {
            if (!locationList.Any())
            {
                return;
            }

            if (locationList.Count > MaxLocationListCount)
            {
                await this.ImportLargeLocationList(locationList);
                return;
            }

            var jsonLocationList =
                from location in locationList
                select CreateJsonObjectFromLocation(location);

            string js = string.Format(
                "await map.addLocationList({0});",
                JsonConvert.SerializeObject(jsonLocationList));

            await this.RunJavaScriptAsync(js);
        }

        /// <summary>
        /// Imports large location list
        /// </summary>
        /// <param name="locationList">large location list to import</param>
        /// <returns>task to wait on</returns>
        private async Task ImportLargeLocationList(List<Location> locationList)
        {
            for (int i = 0; i < locationList.Count; i += MaxLocationListCount)
            {
                int remainingCount = Math.Min(MaxLocationListCount, locationList.Count - i);
                var locationSubList = locationList.GetRange(i, remainingCount);

                await this.AddLocationList(locationSubList);
            }
        }

        /// <summary>
        /// Updates position and other infos of a single location
        /// </summary>
        /// <param name="location">location to update</param>
        public void UpdateLocation(Location location)
        {
            object jsonLocation = CreateJsonObjectFromLocation(location);

            string js = string.Format(
                "await map.updateLocation({0});",
                JsonConvert.SerializeObject(jsonLocation));

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Removes a single location from the map view
        /// </summary>
        /// <param name="locationId">location ID of location to remove</param>
        public void RemoveLocation(string locationId)
        {
            string js = $"map.removeLocation(\"{locationId}\");";
            this.RunJavaScript(js);
        }

        /// <summary>
        /// Samples track point heights from actual map and returns it
        /// </summary>
        /// <param name="track">track to modify</param>
        /// <returns>ground profile heights</returns>
        public async Task<double[]> SampleTrackHeights(Track track)
        {
            // need an initialized map in order to sample data
            await this.taskCompletionSourceMapInitialized.Task;

            var trackPointsList =
                track.TrackPoints.SelectMany(x => new double[]
                {
                    x.Longitude,
                    x.Latitude,
                    x.Altitude ?? 0.0,
                });

            var trackJsonObject = new
            {
                id = track.Id,
                listOfTrackPoints = trackPointsList,
            };

            string js = $"map.sampleTrackHeights({JsonConvert.SerializeObject(trackJsonObject)});";

            this.taskCompletionSourceSampleTrackHeights =
                new TaskCompletionSource<double[]>();

            this.RunJavaScript(js);

            return await this.taskCompletionSourceSampleTrackHeights.Task;
        }

        /// <summary>
        /// Adds new track with given name and map points
        /// </summary>
        /// <param name="track">track to add</param>
        /// <returns>task to wait on</returns>
        public async Task AddTrack(Track track)
        {
            if (!track.TrackPoints.Any() && !track.IsLiveTrack)
            {
                return;
            }

            // round lat/long to 6 digits, getting about 0,1m accuracy
            Func<double, decimal> getRoundedLatLong =
                latlong => Math.Round((decimal)latlong, 6);

            // round altitude to 0,1m accuracy
            Func<double, decimal> getRoundedHeightFromAltitude =
                altitude => Math.Round((decimal)altitude, 1);

            var trackPointsList =
                track.TrackPoints.SelectMany(x => new decimal[]
                {
                    getRoundedLatLong(x.Longitude),
                    getRoundedLatLong(x.Latitude),
                    getRoundedHeightFromAltitude(x.Altitude ?? 0.0),
                });

            List<decimal> timePointsList = null;

            long? trackStart = null;

            var firstTrackPoint = track.TrackPoints.FirstOrDefault();
            if (firstTrackPoint != null &&
                firstTrackPoint.Time.HasValue)
            {
                var startTime = firstTrackPoint.Time.Value;

                trackStart = (long)(startTime.ToUnixTimeMilliseconds() / 1000.0);

                // round time delta to 1/10 seconds, or 10Hz sample rate
                Func<TrackPoint, decimal> getTimeDeltaFromTrackPoint =
                    x => x.Time.HasValue
                    ? Math.Round((decimal)(x.Time.Value - startTime).TotalSeconds, 1)
                    : 0m;

                timePointsList = track.TrackPoints
                    .Select(getTimeDeltaFromTrackPoint)
                    .ToList();
            }

            IEnumerable<decimal> groundHeightProfileList = track.GroundHeightProfile.Any()
                ? track.GroundHeightProfile.Select(getRoundedHeightFromAltitude)
                : null;

            string color = track.IsFlightTrack && !track.IsLiveTrack
                ? null
                : track.Color;

            var trackJsonObject = new
            {
                id = track.Id,
                name = track.Name,
                isFlightTrack = track.IsFlightTrack,
                isLiveTrack = track.IsLiveTrack,
                listOfTrackPoints = trackPointsList,
                listOfTimePoints = timePointsList,
                trackStart,
                groundHeightProfile = groundHeightProfileList,
                color,
            };

            string js = $"await map.addTrack({JsonConvert.SerializeObject(trackJsonObject)});";

            await this.RunJavaScriptAsync(js);
        }

        /// <summary>
        /// Updates track infos like name and color
        /// </summary>
        /// <param name="track">track to update</param>
        public void UpdateTrack(Track track)
        {
            var trackJsonObject = new
            {
                id = track.Id,
                name = track.Name,
                color = track.IsFlightTrack ? null : track.Color,
            };

            string js = $"map.updateTrack({JsonConvert.SerializeObject(trackJsonObject)});";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Updates live track data for a track
        /// </summary>
        /// <param name="data">live track data with updated track points</param>
        public void UpdateLiveTrack(LiveTrackData data)
        {
            var trackJsonObject = new
            {
                id = data.ID,
                name = data.Name,
                description = data.Description,
                trackStart = data.TrackStart.ToUnixTimeMilliseconds() / 1000.0,
                trackPoints = data.TrackPoints.Select(
                    trackPoint => new
                    {
                        latitude = trackPoint.Latitude,
                        longitude = trackPoint.Longitude,
                        altitude = trackPoint.Altitude,
                        offset = trackPoint.Offset,
                    }).ToArray(),
            };

            string js = $"map.updateLiveTrack({JsonConvert.SerializeObject(trackJsonObject)});";

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Zooms to track on map
        /// </summary>
        /// <param name="track">track to zoom to</param>
        public void ZoomToTrack(Track track)
        {
            string js = $"map.zoomToTrack('{track.Id}');";
            this.RunJavaScript(js);
        }

        /// <summary>
        /// Removes track from map
        /// </summary>
        /// <param name="track">track to remove</param>
        public void RemoveTrack(Track track)
        {
            string js = $"map.removeTrack('{track.Id}');";
            this.RunJavaScript(js);
        }

        /// <summary>
        /// Clears all tracks from map
        /// </summary>
        public void ClearAllTracks()
        {
            this.RunJavaScript("map.clearAllTracks();");
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
                altitude = point.Altitude,
                displayLatitude = GeoDataFormatter.FormatLatLong(point.Latitude, this.CoordinateDisplayFormat),
                displayLongitude = GeoDataFormatter.FormatLatLong(point.Longitude, this.CoordinateDisplayFormat),
            };

            string js = string.Format(
                "await map.showFindResult({0});",
                JsonConvert.SerializeObject(options));

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Shows flying range by displaying a half transparent cone on the given point and using
        /// some flying range parameters
        /// </summary>
        /// <param name="point">map point to display cone at</param>
        /// <param name="parameters">flying range parameters</param>
        public void ShowFlyingRange(MapPoint point, FlyingRangeParameters parameters)
        {
            Debug.Assert(
                point.Altitude.HasValue,
                "point must specify an altitude");

            double finalAltitude =
                (point.Altitude ?? 0.0) + parameters.AltitudeOffset;

            var options = new
            {
                latitude = point.Latitude,
                longitude = point.Longitude,
                displayLatitude = GeoDataFormatter.FormatLatLong(point.Latitude, this.CoordinateDisplayFormat),
                displayLongitude = GeoDataFormatter.FormatLatLong(point.Longitude, this.CoordinateDisplayFormat),
                altitude = finalAltitude,
                glideRatio = parameters.GlideRatio,
                gliderSpeed = parameters.GliderSpeed,
                windDirection = parameters.WindDirection,
                windSpeed = parameters.WindSpeed,
            };

            string js = string.Format(
                "map.showFlyingRange({0});",
                JsonConvert.SerializeObject(options));

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Updates scene by requesting re-rendering the scene.
        /// </summary>
        public void UpdateScene()
        {
            this.RunJavaScript("map.updateScene();");
        }

        /// <summary>
        /// Can be called to signal the MapView that network connectivity is available or not.
        /// </summary>
        /// <param name="isAvailable">true when available, false when not</param>
        public void OnNetworkConnectivityChanged(bool isAvailable)
        {
            if (!this.MapInitializedTask.IsCompleted)
            {
                return;
            }

            string js = string.Format(
                "map.onNetworkConnectivityChanged({0});",
                isAvailable ? "true" : "false");

            this.RunJavaScript(js);
        }

        /// <summary>
        /// Runs JavaScript code, in main thread; returns immediately without waiting for the
        /// script to complete. The script may start with the JavaScript async keyword.
        /// </summary>
        /// <param name="js">JavaScript code snippet</param>
        private void RunJavaScript(string js)
        {
            Debug.WriteLine("run js: " + js.Substring(0, Math.Min(80, js.Length)));

            // when the command uses await, add an IIFE around it; see:
            // https://flaviocopes.com/javascript-iife/
            if (js.StartsWith("await"))
            {
                js = "(async function() { " + js + " })();";
            }

            Device.BeginInvokeOnMainThread(() => this.Eval(js));
        }

        /// <summary>
        /// Runs JavaScript code, in main thread, and returns the result. Waits until the
        /// JavaScript code finishes. The script may start with the JavaScript async keyword.
        /// </summary>
        /// <param name="js">JavaScript code snippet</param>
        /// <returns>JavaScript object as JSON formatted string</returns>
        private async Task<string> RunJavaScriptAsync(string js)
        {
            Debug.WriteLine("run js: " + js.Substring(0, Math.Min(80, js.Length)));

            // when the command uses await, add an IIFE around it; see:
            // https://flaviocopes.com/javascript-iife/
            if (js.StartsWith("await"))
            {
                js = "(async function() { const result = " + js + " })();";
            }

            string result = await Device.InvokeOnMainThreadAsync(
                () => this.EvaluateJavaScriptAsync(js));

            return result;
        }

        /// <summary>
        /// Registers callback functions from JavaScript to C#
        /// </summary>
        private void RegisterWebViewCallbacks()
        {
            var callbackHandler = new WebViewCallbackSchemaHandler(this);

            callbackHandler.RegisterHandler(
                "onMapInitialized",
                (jsonParameters) => this.OnMapInitialized());

            callbackHandler.RegisterHandler(
                "onShowLocationDetails",
                (jsonParameters) => this.ShowLocationDetails?.Invoke(jsonParameters.Trim('\"')));

            callbackHandler.RegisterHandler(
                "onNavigateToLocation",
                (jsonParameters) => this.NavigateToLocation?.Invoke(jsonParameters.Trim('\"')));

            callbackHandler.RegisterHandler(
                "onShareMyLocation",
                (jsonParameters) => this.ShareMyLocation?.Invoke());

            callbackHandler.RegisterHandler(
                "onAddFindResult",
                this.OnAddFindResult);

            callbackHandler.RegisterHandler(
                "onLongTap",
                this.OnLongTap);

            callbackHandler.RegisterHandler(
                "onAddTourPlanLocation",
                (jsonParameters) => this.AddTourPlanLocation?.Invoke(jsonParameters.Trim('\"')));

            callbackHandler.RegisterHandler(
                "onUpdateLastShownLocation",
                this.OnUpdateLastShownLocation);

            callbackHandler.RegisterHandler(
                "onSetLocationAsCompassTarget",
                this.OnSetLocationAsCompassTarget);

            callbackHandler.RegisterHandler(
                "onSampledTrackHeights",
                this.OnSampledTrackHeights);

            callbackHandler.RegisterHandler(
                "onExportLayer",
                this.OnExportLayer);

            callbackHandler.RegisterHandler(
                "onConsoleErrorMessage",
                this.OnConsoleErrorMessage);
        }

        /// <summary>
        /// Called when the "mapInitialized" callback has been sent from JavaScript.
        /// </summary>
        private void OnMapInitialized()
        {
            Debug.Assert(
                this.taskCompletionSourceMapInitialized != null,
                "task completion source must have been created");

            this.taskCompletionSourceMapInitialized?.SetResult(true);
        }

        /// <summary>
        /// Called when the "onAddFindResult" callback has been sent from JavaScript.
        /// </summary>
        /// <param name="jsonParameters">find result parameters as JSON</param>
        private void OnAddFindResult(string jsonParameters)
        {
            var parameters = JsonConvert.DeserializeObject<AddFindResultParameter>(jsonParameters);

            if (parameters != null)
            {
                var point = new MapPoint(
                    parameters.Latitude,
                    parameters.Longitude,
                    parameters.Altitude);

                this.AddFindResult?.Invoke(parameters.Name, point);
            }
        }

        /// <summary>
        /// Called when the "onLongTap" callback has been sent from JavaScript.
        /// </summary>
        /// <param name="jsonParameters">long tap parameters as JSON</param>
        private void OnLongTap(string jsonParameters)
        {
            var longTapParameters = JsonConvert.DeserializeObject<LongTapParameter>(jsonParameters);

            if (longTapParameters != null)
            {
                var longTapPoint = new MapPoint(
                    longTapParameters.Latitude,
                    longTapParameters.Longitude,
                    Math.Round(longTapParameters.Altitude));

                this.LongTap?.Invoke(longTapPoint);
            }
        }

        /// <summary>
        /// Called when the "onUpdateLastShownLocation" callback has been sent from JavaScript.
        /// </summary>
        /// <param name="jsonParameters">update last shown location parameters as JSON</param>
        private void OnUpdateLastShownLocation(string jsonParameters)
        {
            // this action uses the same parameters as the onLongTap action
            var updateLastShownLocationParameters =
                JsonConvert.DeserializeObject<UpdateLastShownLocationParameter>(jsonParameters);

            if (updateLastShownLocationParameters != null)
            {
                var updateLastShownLocationPoint = new MapPoint(
                    updateLastShownLocationParameters.Latitude,
                    updateLastShownLocationParameters.Longitude,
                    Math.Round(updateLastShownLocationParameters.Altitude));

                int viewingDistance = updateLastShownLocationParameters.ViewingDistance;

                this.UpdateLastShownLocation?.Invoke(updateLastShownLocationPoint, viewingDistance);
            }
        }

        /// <summary>
        /// Called when the "onSetLocationAsCompassTarget" callback has been sent from JavaScript.
        /// </summary>
        /// <param name="jsonParameters">location ID as JSON string; may be null</param>
        private void OnSetLocationAsCompassTarget(string jsonParameters)
        {
            string locationId = jsonParameters?.Trim('\"');

            this.SetLocationAsCompassTarget?.Invoke(locationId);
        }

        /// <summary>
        /// Called when SampleTrackHeights() call returns data via "onSampledTrackHeights"
        /// JavaScript call
        /// </summary>
        /// <param name="jsonParameters">track point heights as JSON array</param>
        private void OnSampledTrackHeights(string jsonParameters)
        {
            var trackPointHeights = JsonConvert.DeserializeObject<double[]>(jsonParameters);
            this.taskCompletionSourceSampleTrackHeights.SetResult(trackPointHeights);
        }

        /// <summary>
        /// Called when ExportLayer() was called
        /// </summary>
        /// <param name="jsonParameters">
        /// KMZ byte stream as Base64 encoded data formatted as JSON
        /// </param>
        private void OnExportLayer(string jsonParameters)
        {
            string base64Data = jsonParameters.Trim('"').Replace(' ', '+');
            byte[] kmzData = null;
            try
            {
                if (!string.IsNullOrEmpty(base64Data))
                {
                    kmzData = Convert.FromBase64String(base64Data);
                }
            }
            catch (Exception ex)
            {
                this.LogErrorAction?.Invoke(ex);
            }

            this.taskCompletionSourceExportLayer.SetResult(kmzData);
        }

        /// <summary>
        /// Called when the JavaScript console.error() message is called.
        /// </summary>
        /// <param name="message">message text</param>
        private void OnConsoleErrorMessage(string message)
        {
            this.LogErrorAction?.Invoke(
                new InvalidOperationException(
                    "JavaScript error: " + message));
        }
    }
}
