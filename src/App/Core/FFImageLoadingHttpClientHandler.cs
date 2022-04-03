using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// HttpClientHandler implementation for FFImageLoading that adds a Referer header to fix
    /// loading some favicon images that need the header.
    /// </summary>
    public class FFImageLoadingHttpClientHandler : HttpClientHandler
    {
        /// <summary>
        /// Overrides sending requests
        /// </summary>
        /// <param name="request">request to send</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>response message</returns>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            const string UriPathSeparator = "/";

            // the referer only contains the Url without the path
            string refererUrl = request.RequestUri
                .GetLeftPart(System.UriPartial.Authority) + UriPathSeparator;

            request.Headers.Add("Referer", refererUrl);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
