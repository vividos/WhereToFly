using Refit;
using System.Threading.Tasks;

namespace WhereToFly.App.Logic
{
    /// <summary>
    /// Data service that communicates with the backend data service (the project
    /// WhereToFly.WebApi.Core).
    /// </summary>
    public class BackendDataService
    {
        /// <summary>
        /// Interface to REST service for our backend web API
        /// </summary>
        internal interface IBackendWebApi
        {
            /// <summary>
            /// Returns a favicon URL representing the icon for a given website.
            /// </summary>
            /// <param name="websiteUrl">website to get favicon URL</param>
            /// <returns>favicon URL</returns>
            [Get("/api/FaviconUrl?websiteUrl={websiteUrl}")]
            Task<string> GetFaviconUrlAsync(string websiteUrl);
        }

#pragma warning disable S1075 // URIs should not be hardcoded
        /// <summary>
        /// Base URL for the WebApi REST web service
        /// </summary>
        private const string BaseUrl = "https://wheretoflywebapi.azurewebsites.net";
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// Access to backend WebApi REST API
        /// </summary>
        private readonly IBackendWebApi backendWebApi;

        /// <summary>
        /// Creates a new backend data service object
        /// </summary>
        public BackendDataService()
        {
            this.backendWebApi = RestService.For<IBackendWebApi>(BaseUrl);
        }

        /// <summary>
        /// Returns a favicon URL representing the icon for a given website.
        /// </summary>
        /// <param name="websiteUrl">website to get favicon URL</param>
        /// <returns>favicon URL</returns>
        public async Task<string> GetFaviconUrlAsync(string websiteUrl)
        {
            return await this.backendWebApi.GetFaviconUrlAsync(websiteUrl);
        }
    }
}
