using System.Net.Http;
using System.Net.Http.Headers;
using LocalBalancer.Abstractions;

namespace LocalBalancer
{
    /// <summary>
    /// Creates a new <see cref="HttpClient"/> from <seealso cref="IHttpClientFactory"/> using the <seealso cref="NetworkNodeSettings.NetworkAddress"/> as name.
    /// Sets the headers configured through <see cref="IRequestHeadersProvider"/>.
    /// the <see cref="HttpClient.BaseAddress"/> will be <seealso cref="NetworkNodeSettings.NetworkAddress"/>.
    /// </summary>
    public class BalancedHttpClientProvider : IBalancedHttpClientProvider
    {
        private readonly IRequestHeadersProvider _headerProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBalancer _nodeProvider;

        public BalancedHttpClientProvider(
            IHttpClientFactory httpClientFactory,
            IBalancer nodeSelector,
            IRequestHeadersProvider nodeAuthenticationProvider)
        {
            _nodeProvider = nodeSelector;
            _headerProvider = nodeAuthenticationProvider;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Creates the <see cref="HttpClient"/> instance based on the given node settings
        /// Sets the common headers <see cref="SetCommonHeaders(HttpClient)"/>
        /// and then finally sets the Auth Headers <see cref="IRequestHeadersProvider"/>
        /// </summary>
        /// <returns></returns>
        public HttpClient CreateClient(NetworkNodeSettings node)
        {
            HttpClient client = _httpClientFactory.CreateClient(node.NetworkAddress.ToString());

            client.BaseAddress = node.NetworkAddress;

            this.SetCommonHeaders(client);

            _headerProvider.SetHeaders(node, client);

            return client;
        }

        /// <summary>
        /// Creates a new HttpClient using currently active node settings
        /// Invokes <see cref="this.CreateClient(node)"/>
        /// </summary>
        /// <returns></returns>
        public HttpClient CreateClient()
        {
            return this.CreateClient(_nodeProvider.GetCurrentNodeSettings());
        }

        /// <summary>
        /// If the request uri needs to be changed prior to invoking the request,
        /// it should be done in this method
        /// </summary>
        /// <param name="uriMagic">if you wish to do any URI transformations, this is the way to do it.</param>
        /// <returns></returns>
        public virtual string GetRequestUri(string requestUri)
        {
            return requestUri;
        }

        /// <summary>
        /// Default implementation sets the Accept:JSON content type
        /// </summary>
        /// <param name="client"></param>
        protected virtual void SetCommonHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.JsonContentType));
        }
    }
}