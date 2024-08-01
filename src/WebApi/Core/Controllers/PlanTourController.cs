using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.Shared.Model;
using WhereToFly.WebApi.Core.Services;

namespace WhereToFly.WebApi.Core.Controllers
{
    /// <summary>
    /// Controller for planning tours using multiple waypoints. Only waypoints with the flag
    /// IsPlanTourLocation can be used for tour planning.
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class PlanTourController : ControllerBase
    {
        /// <summary>
        /// Logging instance
        /// </summary>
        private readonly ILogger<PlanTourController> logger;

        /// <summary>
        /// Tour planning service
        /// </summary>
        private readonly PlanTourService service;

        /// <summary>
        /// Creates a new controller to plan tours
        /// </summary>
        /// <param name="logger">logger instance to use</param>
        /// <param name="service">tour planning service</param>
        public PlanTourController(
            ILogger<PlanTourController> logger,
            PlanTourService service)
        {
            this.logger = logger;
            this.service = service;
        }

        /// <summary>
        /// POST api/PlanTour
        /// Plans a tour with given tour planning parameters and returns it.
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        /// <returns>planned tour</returns>
        [HttpPost]
        [Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PlannedTour>> Get(
            [FromBody] PlanTourParameters planTourParameters)
        {
            this.logger.LogDebug(
                "Planning tour with {Count} waypoints...",
                planTourParameters.WaypointIdList.Count);

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var result = await this.service.PlanTour(planTourParameters);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception while planning tour");

                return this.BadRequest(ex.Message);
            }
            finally
            {
                watch.Stop();

                this.logger.LogDebug(
                    "Planning tour took {ElapsedMilliseconds} ms.",
                    watch.ElapsedMilliseconds);
            }
        }
    }
}
