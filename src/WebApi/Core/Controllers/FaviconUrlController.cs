using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WhereToFly.WebApi.Logic;

namespace WhereToFly.WebApi.Core.Controllers
{
    /// <summary>
    /// Controller to return favicon URL links for web pages
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FaviconUrlController : Controller
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger<AppConfigController> logger;

        /// <summary>
        /// Cache for favicon URLs
        /// </summary>
        private readonly FaviconUrlCache faviconUrlCache = new FaviconUrlCache();

        /// <summary>
        /// Creates a new favicon URL controller
        /// </summary>
        /// <param name="logger">logging instance</param>
        public FaviconUrlController(ILogger<AppConfigController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// GET: api/FaviconUrl?websiteUrl=URL
        /// Returns a favicon URL representing the icon for a given website.
        /// </summary>
        /// <param name="websiteUrl">website to get favicon URL</param>
        /// <returns>favicon URL</returns>
        [HttpGet]
        public async Task<string> Get(string websiteUrl)
        {
            this.logger.LogInformation($"WhereToFly favicon URL request, with website URL: {websiteUrl}");

            try
            {
                return await this.faviconUrlCache.GetFaviconUrlAsync(new Uri(websiteUrl));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"error while retrieving favicon URL for website: {websiteUrl}");

                return await Task.FromResult(string.Empty);
            }
        }
    }
}
