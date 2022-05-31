using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LocalBalancer.Abstractions;
using Microsoft.Extensions.Logging;

namespace LocalBalancer.Http
{
    /// <summary>
    /// Wraps <see cref="HttpClient"/> with the balancer
    /// </summary>
    public class HttpRestClient : IRestApiClient
    {
        private readonly ILogger<HttpRestClient> _logger;
        private readonly IBalancedNetworkPolicy<HttpResponseMessage> _nodePolicy;
        private readonly IBalancedHttpClientProvider _provider;

        public HttpRestClient(
            IBalancedNetworkPolicy<HttpResponseMessage> nodePolicy,
            IBalancedHttpClientProvider provider,
            ILogger<HttpRestClient> logger)
        {
            _nodePolicy = nodePolicy;
            _provider = provider;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // prevent retries on spending requests to prevent any double spending
            if (this.ShouldSkipRetry(request))
            {
                _logger.LogDebug("request={@request} on route={route} will not be retried", request);

                return await _nodePolicy.ExecuteOnceAsync(
                    // executes this request just once
                    node => this.SendInternalAsync(request, cancellationToken), request.RequestUri.ToString());
            }

            HttpResponseMessage response = await _nodePolicy.ExecuteAsync(
                // which will create a new rpc client every time we do a retry
                node => this.SendInternalAsync(request, cancellationToken),
                    request.RequestUri.ToString(),
            // : provides cancellation capabilities to the policy
            cancellationToken);

            return response;
        }

        protected virtual bool ShouldSkipRetry(HttpRequestMessage request)
        {
            return false;
        }

        private async Task<HttpResponseMessage> SendInternalAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            try
            {
                HttpClient client = _provider.CreateClient();

                response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (ComponentSettings.TransientErrors.Contains(response.StatusCode))
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogError("Transiet error: {responseContent}", responseContent);

                    throw new HttpRequestException(response.ReasonPhrase);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Transient Error calling api {@statusCode} {method} {requestUri} {@error}", response?.StatusCode, request.Method, request.RequestUri, ex.Message);

                throw;
            }
        }
    }
}