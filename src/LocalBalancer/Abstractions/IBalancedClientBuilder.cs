using System.Net.Http;

namespace LocalBalancer.Abstractions
{
    /// <summary>
    /// Contract of the local balacner <see cref="HttpClient"/> builder
    /// </summary>
    public interface IHttpClientBuilder
    {
        HttpClient BuildHttpClient();
    }
}