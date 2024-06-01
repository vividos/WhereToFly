using WhereToFly.App.MapView;
using WhereToFly.App.Models;
using WhereToFly.App.Views;
using WhereToFly.Geo.Model;

namespace WhereToFly.App
{
    /// <summary>
    /// The app's globally available map services
    /// </summary>
    public interface IAppMapService
    {
        /// <summary>
        /// The one and only map page (displaying the map using CesiumJS)
        /// </summary>
        MapPage MapPage { get; }

        /// <summary>
        /// Access to the map view instance
        /// </summary>
        IMapView MapView { get; }

        /// <summary>
        /// Adds a tour planning location to the current list of locations and opens the planning
        /// dialog.
        /// </summary>
        /// <param name="location">location to add</param>
        /// <returns>task to wait on</returns>
        Task AddTourPlanLocation(Location location);

        /// <summary>
        /// Adds track to map view
        /// </summary>
        /// <param name="track">track to add</param>
        /// <returns>task to wait on</returns>
        Task AddTrack(Track track);

        /// <summary>
        /// Updates map settings on opened MapPage.
        /// </summary>
        void UpdateMapSettings();

        /// <summary>
        /// Updates last shown position in app settings
        /// </summary>
        /// <param name="point">current position</param>
        /// <param name="viewingDistance">current viewing distance; may be unset</param>
        /// <returns>task to wait on</returns>
        Task UpdateLastShownPosition(MapPoint point, int? viewingDistance = null);

        /// <summary>
        /// Sets (or clears) the current compass target
        /// </summary>
        /// <param name="compassTarget">compass target; may be null</param>
        /// <returns>task to wait on</returns>
        Task SetCompassTarget(CompassTarget? compassTarget);

        /// <summary>
        /// Opens app resource URI, e.g. a live waypoint
        /// </summary>
        /// <param name="uri">app resource URI to open</param>
        void OpenAppResourceUri(string uri);

        /// <summary>
        /// Initializes live waypoint refresh service with current location list
        /// </summary>
        /// <returns>task to wait on</returns>
        Task InitLiveWaypointRefreshService();

        /// <summary>
        /// Shows the flight planning disclaimer dialog, when not already shown to the user.
        /// </summary>
        /// <returns>task to wait on</returns>
        Task ShowFlightPlanningDisclaimer();
    }
}
