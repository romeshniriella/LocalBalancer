using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LocalBalancer.Http
{
    /// <summary>
    /// Contract of an Rest API Client
    /// </summary>
    public interface IRestApiClient
    {
        /// <summary>
        /// Sends the <see cref="HttpRequestMessage"/> to the destination.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>the <see cref="HttpResponseMessage"/></returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}