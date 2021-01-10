using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.App.Geo;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Interface to the map view
    /// </summary>
    public interface IMapView
    {
        #region Map settings
        /// <summary>
        /// Gets or sets map imagery type
        /// </summary>
        MapImageryType MapImageryType { get; set; }

        /// <summary>
        /// Gets or sets map overlay type
        /// </summary>
        MapOverlayType MapOverlayType { get; set; }

        /// <summary>
        /// Gets or sets map shading mode
        /// </summary>
        MapShadingMode MapShadingMode { get; set; }

        /// <summary>
        /// Coordinate display format to use for formatting coordinates
        /// </summary>
        CoordinateDisplayFormat CoordinateDisplayFormat { get; set; }
        #endregion

        /// <summary>
        /// Task that can be awaited to get notified when map view is initialized
        /// </summary>
        Task MapInitializedTask { get; }

        /// <summary>
        /// Creates the map view; this must be called before any other methods.
        /// </summary>
        /// <param name="initialCenterPoint">initial center point to be used for map view</param>
        /// <param name="initialZoomLevel">initial zoom level, in 2D zoom level steps</param>
        /// <param name="useEntityClustering">indicates if entity clustering should be used</param>
        /// <returns>task to wait on</returns>
        Task CreateAsync(MapPoint initialCenterPoint, int initialZoomLevel, bool useEntityClustering);

        /// <summary>
        /// Updates the "my location" pin in the map
        /// </summary>
        /// <param name="position">new position to use</param>
        /// <param name="positionAccuracyInMeter">position accuracy, in meter</param>
        /// <param name="speedInKmh">current speed, in km/h</param>
        /// <param name="timestamp">timestamp of location</param>
        /// <param name="zoomToLocation">indicates if view should also zoom to the location</param>
        void UpdateMyLocation(MapPoint position, int positionAccuracyInMeter, double speedInKmh, DateTimeOffset timestamp, bool zoomToLocation);

        #region Layer methods
        /// <summary>
        /// Adds layer to map
        /// </summary>
        /// <param name="layer">layer to add</param>
        void AddLayer(Layer layer);

        /// <summary>
        /// Removes layer; built-in layers can't be removed.
        /// </summary>
        /// <param name="layer">layer to remove</param>
        void RemoveLayer(Layer layer);

        /// <summary>
        /// Clears layer list; built-in layers can't be removed.
        /// </summary>
        void ClearLayerList();

        /// <summary>
        /// Zooms to given layer on map
        /// </summary>
        /// <param name="layer">layer to zoom to</param>
        void ZoomToLayer(Layer layer);

        /// <summary>
        /// Sets new visibility of given layer; IsVisible property of layer is used.
        /// </summary>
        /// <param name="layer">layer to set visibility</param>
        void SetLayerVisibility(Layer layer);

        /// <summary>
        /// Exports given CZML layer as KMZ bytestream
        /// </summary>
        /// <param name="layer">layer to export</param>
        /// <returns>exported KMZ byte stream</returns>
        Task<byte[]> ExportLayerAsync(Layer layer);
        #endregion

        #region Location list methods
        /// <summary>
        /// Adds a single locations to the map, to be displayed as pin.
        /// </summary>
        /// <param name="location">location to add</param>
        void AddLocation(Location location);

        /// <summary>
        /// Adds a list of locations to the map, to be displayed as pins.
        /// </summary>
        /// <param name="locationList">list of locations to add</param>
        void AddLocationList(List<Location> locationList);

        /// <summary>
        /// Updates position and other infos of a single location
        /// </summary>
        /// <param name="location">location to update</param>
        void UpdateLocation(Location location);

        /// <summary>
        /// Removes a single location from the map view
        /// </summary>
        /// <param name="locationId">location ID of location to remove</param>
        void RemoveLocation(string locationId);

        /// <summary>
        /// Clears location list
        /// </summary>
        void ClearLocationList();

        /// <summary>
        /// Zooms to given location
        /// </summary>
        /// <param name="position">position to zoom to</param>
        void ZoomToLocation(MapPoint position);

        /// <summary>
        /// Zooms to rectangle specified by two map points
        /// </summary>
        /// <param name="minPosition">minimum position values</param>
        /// <param name="maxPosition">maximum position values</param>
        void ZoomToRectangle(MapPoint minPosition, MapPoint maxPosition);
        #endregion

        #region Track list methods
        /// <summary>
        /// Adds new track with given name and map points
        /// </summary>
        /// <param name="track">track to add</param>
        void AddTrack(Track track);

        /// <summary>
        /// Removes track from map
        /// </summary>
        /// <param name="track">track to remove</param>
        void RemoveTrack(Track track);

        /// <summary>
        /// Clears all tracks from map
        /// </summary>
        void ClearAllTracks();

        /// <summary>
        /// Zooms to track on map
        /// </summary>
        /// <param name="track">track to zoom to</param>
        void ZoomToTrack(Track track);

        /// <summary>
        /// Samples track point heights from actual map and returns it.
        /// </summary>
        /// <param name="track">track points to use</param>
        /// <returns>sampled ground point elevations for all track points</returns>
        Task<double[]> SampleTrackHeights(Track track);
        #endregion
    }
}
