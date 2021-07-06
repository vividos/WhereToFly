using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WhereToFly.Shared.Model;
using WhereToFly.WebApi.Logic;

namespace WhereToFly.WebApi.Core.Controllers
{
    /// <summary>
    /// Controller for delivering Live Tracks in a standard way. Live tracks contain track points
    /// delivered over time and can be combined to a full track.
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class LiveTrackController : Controller
    {
        /// <summary>
        /// Logging instance
        /// </summary>
        private readonly ILogger<LiveTrackController> logger;

        /// <summary>
        /// Live track cache manager
        /// </summary>
        private readonly LiveTrackCacheManager cacheManager;

        /// <summary>
        /// Creates a new controller to get live track data
        /// </summary>
        /// <param name="logger">logger instance to use</param>
        /// <param name="cacheManager">live track cache manager to use</param>
        public LiveTrackController(ILogger<LiveTrackController> logger, LiveTrackCacheManager cacheManager)
        {
            this.logger = logger;
            this.cacheManager = cacheManager;
        }

        /// <summary>
        /// GET api/LiveTrack/id
        /// Retrieves the current live track data for the given ID. The latest data for the
        /// live track is retrieved when necessary, or a cached copy of the data is used.
        /// </summary>
        /// <param name="id">live track ID</param>
        /// <param name="lastTrackPointTime">
        /// last track point that the client already has received, or null when no track points
        /// are known yet
        /// </param>
        /// <returns>live track query result</returns>
        /// <exception cref="ArgumentException">thrown when invalid live track ID was passed</exception>
        [HttpGet]
        public async Task<LiveTrackQueryResult> Get(
            string id,
            [FromQuery(Name = "time")] DateTimeOffset? lastTrackPointTime)
        {
            this.CheckLiveTrackId(id);

            this.logger.LogDebug($"getting live track with ID: {id}");

            return await this.cacheManager.GetLiveTrackDataAsync(id, lastTrackPointTime);
        }

        /// <summary>
        /// Checks live track ID parameter and throws exception when invalid
        /// </summary>
        /// <param name="liveTrackId">live track ID to check</param>
        private void CheckLiveTrackId(string liveTrackId)
        {
            if (string.IsNullOrEmpty(liveTrackId))
            {
                this.logger.LogWarning($"invalid live track ID: {liveTrackId}");
                throw new ArgumentException(
                    "invalid live track ID",
                    nameof(liveTrackId));
            }
        }
    }
}
