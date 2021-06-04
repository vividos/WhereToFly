using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WhereToFly.Shared.Model;
using WhereToFly.WebApi.Logic;

namespace WhereToFly.WebApi.Core.Controllers
{
    /// <summary>
    /// Controller for delivering Live Waypoints in a standard way. Live waypoints can be of any
    /// type, including weather data for a location, a webcam image, or a GPS tracking device
    /// (such as SPOT or Garmin InReach).
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class LiveWaypointController : Controller
    {
        /// <summary>
        /// Logging instance
        /// </summary>
        private readonly ILogger<LiveWaypointController> logger;

        /// <summary>
        /// Live waypoint cache manager
        /// </summary>
        private readonly LiveWaypointCacheManager cacheManager;

        /// <summary>
        /// Creates a new controller to get live waypoint data
        /// </summary>
        /// <param name="logger">logger instance to use</param>
        /// <param name="cacheManager">cache manager to use</param>
        public LiveWaypointController(ILogger<LiveWaypointController> logger, LiveWaypointCacheManager cacheManager)
        {
            this.logger = logger;
            this.cacheManager = cacheManager;
        }

        /// <summary>
        /// GET api/LiveWaypoint/id
        /// Retrieves the current live waypoint infos for the given ID. The ID completely
        /// describes the live waypoint. The latest data for the waypoint is retrieved when
        /// necessary, or a cached copy of the data.
        /// </summary>
        /// <param name="id">live waypoint ID</param>
        /// <returns>live waypoint query result</returns>
        /// <exception cref="ArgumentException">thrown when invalid live waypoint ID was passed</exception>
        [HttpGet]
        public async Task<LiveWaypointQueryResult> Get(string id)
        {
            this.CheckLiveWaypointId(id);

            this.logger.LogDebug($"getting live waypoint with ID: {id}");

            return await this.cacheManager.GetLiveWaypointData(id);
        }

        /// <summary>
        /// Checks live waypoint ID parameter and throws exception when invalid
        /// </summary>
        /// <param name="liveWaypointId">live waypoint ID to check</param>
        private void CheckLiveWaypointId(string liveWaypointId)
        {
            if (string.IsNullOrEmpty(liveWaypointId))
            {
                this.logger.LogWarning($"invalid live waypoint ID: {liveWaypointId}");
                throw new ArgumentException(
                    "invalid live waypoint ID",
                    nameof(liveWaypointId));
            }
        }
    }
}
