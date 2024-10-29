using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.MapView;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Map view for unit tests
    /// </summary>
    internal class UnitTestMapView : IMapView
    {
        /// <summary>
        /// Task completion source for <see cref="MapInitializedTask"/>
        /// </summary>
        private readonly TaskCompletionSource<bool> tcsMapInitialized = new();

        /// <summary>
        /// List of all layers in the map view
        /// </summary>
        private readonly List<Layer> layerList = [];

        /// <summary>
        /// List of all locations in the map view
        /// </summary>
        private readonly List<Location> locationList = [];

        /// <summary>
        /// List of all tracks in the map view
        /// </summary>
        private readonly List<Track> trackList = [];

        #region Map settings
        /// <inheritdoc />
        public MapImageryType MapImageryType { get; set; }
            = MapImageryType.OpenStreetMap;

        /// <inheritdoc />
        public MapOverlayType MapOverlayType { get; set; }
            = MapOverlayType.None;

        /// <inheritdoc />
        public MapShadingMode MapShadingMode { get; set; }
            = MapShadingMode.CurrentTime;

        /// <inheritdoc />
        public CoordinateDisplayFormat CoordinateDisplayFormat { get; set; }
            = CoordinateDisplayFormat.Format_dd_dddddd;
        #endregion

        /// <summary>
        /// Task that is completed as soon as <see cref="CreateAsync"/>() is called
        /// </summary>
        public Task MapInitializedTask => this.tcsMapInitialized.Task;

        /// <inheritdoc />
        public INearbyPoiService? NearbyPoiService { get; set; }

        /// <summary>
        /// Creates map view; sets <see cref="MapInitializedTask"/> task result
        /// </summary>
        /// <param name="initialCenterPoint">initial center point</param>
        /// <param name="initialViewingDistance">initial viewing distance</param>
        /// <param name="useEntityClustering">
        /// true when entity clustering should be used
        /// </param>
        /// <param name="cesiumIonApiKey">
        /// Cesium Ion API key; when null, no terrain and some Ion based layers are available
        /// </param>
        /// <param name="bingMapsApiKey">
        /// Bing Maps API key; when null, no Bing Maps layer is available
        /// </param>
        /// <returns>task to wait on</returns>
        public Task CreateAsync(
            MapPoint initialCenterPoint,
            int initialViewingDistance,
            bool useEntityClustering,
            string? cesiumIonApiKey,
            string? bingMapsApiKey)
        {
            Debug.WriteLine("MapView: CreateAsync() was called");

            this.tcsMapInitialized.SetResult(true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates current location of user on the map
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
            Debug.WriteLine("MapView: UpdateMyLocation was called");
        }

        /// <inheritdoc />
        public void SetCompassTarget(string title, MapPoint position, bool zoomToPolyline)
        {
            Debug.WriteLine("MapView: SetCompassTarget was called");
        }

        /// <inheritdoc />
        public void SetCompassDirection(string title, int directionAngleInDegrees)
        {
            Debug.WriteLine("MapView: SetCompassDirection was called");
        }

        /// <inheritdoc />
        public void ClearCompass()
        {
            Debug.WriteLine("MapView: ClearCompass was called");
        }

        /// <inheritdoc />
        public Task FindNearbyPois()
        {
            Debug.WriteLine("MapView: FindNearbyPois was called");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void AddNearbyPoiLocations(IEnumerable<Location> nearbyPoiLocations)
        {
            Debug.WriteLine("MapView: AddNearbyPoiLocations was called");
        }

        #region Layer methods

        /// <inheritdoc />
        public Task AddLayer(Layer layer)
        {
            Debug.WriteLine("MapView: AddLayer was called");

            this.layerList.Add(layer);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void RemoveLayer(Layer layer)
        {
            Debug.WriteLine("MapView: RemoveLayer was called");

            this.layerList.Remove(layer);
        }

        /// <inheritdoc />
        public void ClearLayerList()
        {
            Debug.WriteLine("MapView: ClearLayerList was called");

            this.layerList.Clear();
        }

        /// <inheritdoc />
        public void ZoomToLayer(Layer layer)
        {
            Debug.WriteLine("MapView: ZoomToLayer was called");
        }

        /// <inheritdoc />
        public void SetLayerVisibility(Layer layer)
        {
            Debug.WriteLine("MapView: SetLayerVisibility was called");
        }

        /// <inheritdoc />
        public Task<byte[]?> ExportLayerAsync(Layer layer)
        {
            Debug.WriteLine("MapView: SetLayerVisibility was called");

            return Task.FromResult<byte[]?>([42]);
        }
        #endregion

        #region Location list methods

        /// <inheritdoc />
        public void AddLocation(Location location)
        {
            Debug.WriteLine("MapView: AddLocation was called");

            this.locationList.Add(location);
        }

        /// <inheritdoc />
        public Task AddLocationList(List<Location> locationList)
        {
            Debug.WriteLine("MapView: AddLocationList was called");

            this.locationList.AddRange(locationList);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void UpdateLocation(Location location)
        {
            Debug.WriteLine("MapView: UpdateLocation was called");
        }

        /// <inheritdoc />
        public void RemoveLocation(string locationId)
        {
            Debug.WriteLine("MapView: RemoveLocation was called");

            Location? locationToRemove = this.locationList.Find(
                location => location.Id == locationId);

            if (locationToRemove != null)
            {
                this.locationList.Remove(locationToRemove);
            }
        }

        /// <inheritdoc />
        public void ClearLocationList()
        {
            Debug.WriteLine("MapView: ClearLocationList was called");

            this.locationList.Clear();
        }

        /// <inheritdoc />
        public void ZoomToLocation(MapPoint position)
        {
            Debug.WriteLine("MapView: ZoomToLocation was called");
        }

        /// <inheritdoc />
        public void ZoomToRectangle(MapPoint minPosition, MapPoint maxPosition)
        {
            Debug.WriteLine("MapView: ZoomToRectangle was called");
        }

        #endregion

        #region Track list methods

        /// <inheritdoc />
        public Task AddTrack(Track track)
        {
            Debug.WriteLine("MapView: AddTrack was called");

            this.trackList.Add(track);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void UpdateTrack(Track track)
        {
            Debug.WriteLine("MapView: UpdateTrack was called");
        }

        /// <inheritdoc />
        public void RemoveTrack(Track track)
        {
            Debug.WriteLine("MapView: RemoveTrack was called");

            this.trackList.Remove(track);
        }

        /// <inheritdoc />
        public void ClearAllTracks()
        {
            Debug.WriteLine("MapView: ClearAllTracks was called");

            this.trackList.Clear();
        }

        /// <inheritdoc />
        public void ZoomToTrack(Track track)
        {
            Debug.WriteLine("MapView: ZoomToTrack was called");
        }

        /// <inheritdoc />
        public Task<double[]?> SampleTrackHeights(Track track)
        {
            Debug.WriteLine("MapView: SampleTrackHeights was called");

            double[] trackHeights = track.TrackPoints
                .Select(
                    trackPoint => (trackPoint.Altitude ?? 0.0) + 200.0)
                .ToArray();

            return Task.FromResult<double[]?>(trackHeights);
        }
        #endregion
    }
}
