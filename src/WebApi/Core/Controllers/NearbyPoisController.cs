using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;
using WhereToFly.WebApi.Logic;

namespace WhereToFly.WebApi.Core.Controllers
{
    /// <summary>
    /// Controller to find nearby POIs for WhereToFly app
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class NearbyPoisController : ControllerBase
    {
        /// <summary>
        /// Logging instance
        /// </summary>
        private readonly ILogger<NearbyPoisController> logger;

        /// <summary>
        /// Location find manager
        /// </summary>
        private readonly LocationFindManager locationFindManager;

        /// <summary>
        /// Creates a new nearby POIs controller
        /// </summary>
        /// <param name="logger">logger instance</param>
        /// <param name="locationFindManager">location find manager instance to use</param>
        public NearbyPoisController(
            ILogger<NearbyPoisController> logger,
            LocationFindManager locationFindManager)
        {
            this.logger = logger;
            this.locationFindManager = locationFindManager;
        }

        /// <summary>
        /// GET: api/NearbyPois
        /// Returns a list of nearby POIs in the map rectangle with the same integer latitude and
        /// longitude values.
        /// </summary>
        /// <param name="latitude">integer latitude value</param>
        /// <param name="longitude">integer longitude value</param>
        /// <returns>list of locations found</returns>
        [HttpGet]
        public async Task<IEnumerable<Location>> Get(int latitude, int longitude)
        {
            this.logger.LogDebug(
                "finding nearby POIs in latitude {Latitude}, longitude {Longitude} rectangle",
                latitude,
                longitude);

            return await this.locationFindManager.GetInRectAsync(latitude, longitude);
        }
    }
}
