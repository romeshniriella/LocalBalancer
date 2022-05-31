using System;
using System.Net.Http;

namespace LocalBalancer.Abstractions
{
    public interface IBalancedHttpClientProvider
    {
        HttpClient CreateClient();

        string GetRequestUri(string requestUri);
    }
}