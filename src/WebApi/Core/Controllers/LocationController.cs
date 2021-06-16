using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;
using WhereToFly.WebApi.Logic;

namespace WhereToFly.WebApi.Core.Controllers
{
    /// <summary>
    /// Controller to find locations for WhereToFly app
    /// </summary>
    [Route("api/location")]
    [Produces("application/json")]
    public class LocationController : ControllerBase
    {
        /// <summary>
        /// Logging instance
        /// </summary>
        private readonly ILogger<LocationController> logger;

        /// <summary>
        /// Location find manager
        /// </summary>
        private readonly LocationFindManager locationFindManager;

        /// <summary>
        /// Creates a new location controller
        /// </summary>
        /// <param name="logger">logger instance</param>
        /// <param name="locationFindManager">location find manager instance to use</param>
        public LocationController(ILogger<LocationController> logger, LocationFindManager locationFindManager)
        {
            this.logger = logger;
            this.locationFindManager = locationFindManager;
        }

        /// <summary>
        /// GET: api/location
        /// Returns a list of locations in the range of the given point
        /// </summary>
        /// <param name="latitude">latitude of point</param>
        /// <param name="longitude">longitude of point</param>
        /// <param name="rangeInKm">range around point, in km</param>
        /// <returns>list of locations found</returns>
        [HttpGet]
        public async Task<IEnumerable<Location>> Get(double latitude, double longitude, double rangeInKm)
        {
            this.logger.LogDebug(
                $"finding locations around latitude {latitude}, longitude {longitude}, in range {rangeInKm} km");

            return await this.locationFindManager.FindAsync(
                new MapPoint(latitude, longitude),
                rangeInKm * 1000.0);
        }

        /// <summary>
        /// GET api/location/{locationId}
        /// Returns a specific location with given location ID
        /// </summary>
        /// <param name="locationID">location ID</param>
        /// <returns>location object</returns>
        [HttpGet("{locationID}")]
        public async Task<Location> Get(string locationID)
        {
            return await this.locationFindManager.GetAsync(locationID)
                ?? throw new Exception("location not found");
        }
    }
}
