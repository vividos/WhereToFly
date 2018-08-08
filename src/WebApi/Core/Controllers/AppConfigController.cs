using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WhereToFly.Shared.Model;

namespace WhereToFly.WebApi.Core.Controllers
{
    /// <summary>
    /// Controller to deliver app config for WhereToiFly app
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AppConfigController : Controller
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger<AppConfigController> logger;

        /// <summary>
        /// App config object to use
        /// </summary>
        private readonly AppConfig appConfig;

        /// <summary>
        /// Creates a new app config controller
        /// </summary>
        /// <param name="logger">logging instance</param>
        public AppConfigController(ILogger<AppConfigController> logger)
        {
            this.logger = logger;

            this.appConfig = new AppConfig
            {
                CesiumIonApiKey = string.Empty,
                BingMapsApiKey = string.Empty,
            };
        }

        /// <summary>
        /// GET: api/AppConfig/appVersion
        /// Returns current WhereToFly app configuration.
        /// </summary>
        /// <param name="appVersion">version of the app that wants to get app config</param>
        /// <returns>app config object</returns>
        [HttpGet]
        public AppConfig Get(string appVersion)
        {
            this.logger.LogInformation($"WhereToFly app config request, with version {appVersion}");

            return this.appConfig;
        }
    }
}
