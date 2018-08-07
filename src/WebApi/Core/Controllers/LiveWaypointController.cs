using Microsoft.AspNetCore.Mvc;
using WhereToFly.WebApi.Logic.Services;

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
        /// GET api/values/5
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
                return this.BadRequest();
            }

            var service = new FindMeSpotTrackerDataService();

            var liveWaypointData = service.GetDataAsync(id);

            return new JsonResult(liveWaypointData);
        }
    }
}
