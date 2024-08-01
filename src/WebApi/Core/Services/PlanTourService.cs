using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;
using WhereToFly.WebApi.Logic.TourPlanning;

namespace WhereToFly.WebApi.Core.Services
{
    /// <summary>
    /// Service for planning tours. Depending on the plan tour parameters,
    /// uses the <see cref="OpenRouteService"/> for general routing or the
    /// specialized <see cref="PlanTourEngine"/>.
    /// </summary>
    public class PlanTourService
    {
        /// <summary>
        /// Plan tour engine
        /// </summary>
        private readonly PlanTourEngine engine;

        /// <summary>
        /// Open route service
        /// </summary>
        private readonly OpenRouteService openRouteService;

        /// <summary>
        /// Creates a new plan tour service
        /// </summary>
        /// <param name="config">configuration object</param>
        public PlanTourService(IConfiguration config)
        {
            // confiugre tour planning engine
            this.engine = new PlanTourEngine();

            var logicAssembly = typeof(PlanTourEngine).Assembly;
            var kmlStream = logicAssembly.GetManifestResourceStream(
                "WhereToFly.WebApi.Logic.Assets.PlanTourPaths.kml");

            if (kmlStream != null)
            {
                this.engine.LoadGraph(kmlStream);
            }

            // configure open route service
            string apiKey = config["OPENROUTESERVICE_API_KEY"] ?? string.Empty;
            this.openRouteService = new OpenRouteService(apiKey);
        }

        /// <summary>
        /// Plans a tour using the given plan tour parameters
        /// </summary>
        /// <param name="planTourParameters">plan tour parameters</param>
        /// <returns>planned tour</returns>
        public async Task<PlannedTour> PlanTour(
            PlanTourParameters planTourParameters)
        {
            if (planTourParameters.WaypointIdList.TrueForAll(
                waypointId => waypointId.StartsWith("wheretofly-")) &&
                planTourParameters.WaypointLocationList.Count == 0)
            {
                return this.engine.PlanTour(planTourParameters);
            }

            var waypointList = planTourParameters.WaypointLocationList.Select(
                waypointLocation => waypointLocation.MapLocation);

            Track track = await this.openRouteService.GetDirections(
                waypointList,
                new OpenRouteService.Options
                {
                    Profile = OpenRouteService.RouteProfile.FootHiking,
                });

            return new PlannedTour
            {
                TotalDuration = track.Duration,
                MapPointList = track.TrackPoints.Select(
                    trackPoint => new MapPoint(trackPoint.Latitude, trackPoint.Longitude)).ToList(),
                TourEntriesList = new List<PlannedTourEntry>
                {
                    new PlannedTourEntry(
                        planTourParameters.WaypointLocationList.First().Id,
                        planTourParameters.WaypointLocationList.Last().Id)
                    {
                        DistanceInKm = track.LengthInMeter / 1000.0,
                        Duration = track.Duration,
                        TrackStartIndex = 0,
                    },
                }
            };
        }
    }
}
