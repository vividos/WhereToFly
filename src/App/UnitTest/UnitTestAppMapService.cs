using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.App.MapView;
using WhereToFly.App.Models;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Unit test implementation for the app map services
    /// </summary>
    internal class UnitTestAppMapService : IAppMapService
    {
        /// <inheritdoc />
        public IMapView MapView { get; } = new UnitTestMapView();

        /// <inheritdoc />
        public Task AddTourPlanLocation(Location location)
        {
            Debug.WriteLine($"Adding tour plan location, ID={location.Id}, Name={location.Name}");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task AddTrack(Track track)
        {
            Debug.WriteLine($"Adding track, ID={track.Id}, Name={track.Name}");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void UpdateMapSettings()
        {
            Debug.WriteLine("Updating map settings");
        }

        /// <inheritdoc />
        public Task UpdateLastShownPosition(MapPoint point, int? viewingDistance = null)
        {
            Debug.WriteLine($"Updating last shown position, at {point}, distance {viewingDistance} m");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetCompassTarget(CompassTarget? compassTarget)
        {
            Debug.WriteLine($"Setting compass target, to {compassTarget}");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void OpenAppResourceUri(string uri)
        {
            Debug.WriteLine($"Opening app resource URI; {uri}");
        }

        /// <inheritdoc />
        public Task InitLiveWaypointRefreshService()
        {
            Debug.WriteLine("Initing live waypoint refresh service");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ShowFlightPlanningDisclaimer()
        {
            Debug.WriteLine("Showing flight planning disclaimer");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ClearTempPlanTourLocations()
        {
            Debug.WriteLine("Clearing temporary plan tour locations");
            return Task.CompletedTask;
        }
    }
}
