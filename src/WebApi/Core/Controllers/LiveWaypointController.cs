using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WhereToFly.WebApi.Logic;

namespace LiveWaypoints.Controllers
{
    /// <summary>
    /// Controller for delivering Live Waypoints in a standard way. Live waypoints can be of any
    /// type, including weather data for a location, a webcam image, or a GPS tracking device
    /// (such as SPOT or Garmin InReach).
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
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
        /// GET api/livewaypoint/id
        /// Retrieves the current live waypoint infos for the given ID. The ID completely
        /// describes the live waypoint. The latest data for the waypoint is retrieved when
        /// necessary, or a cached copy of the data.
        /// </summary>
        /// <param name="id">the Live Waypoint ID</param>
        /// <returns>live waypoint data</returns>
        /// <response code="400">An invalid live waypoint ID was passed</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public IActionResult Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                this.logger.LogWarning($"invalid live waypoint ID: {id}");
                return this.BadRequest();
            }

            this.logger.LogDebug($"getting live waypoint with ID: {id}");

            var liveWaypointData = this.cacheManager.GetLiveWaypointData(id);

            return new JsonResult(liveWaypointData.Result);
        }
    }
}
