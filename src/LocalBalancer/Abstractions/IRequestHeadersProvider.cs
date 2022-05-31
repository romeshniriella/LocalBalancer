using System.Net.Http;

namespace LocalBalancer.Abstractions
{
    public interface IRequestHeadersProvider
    {
        void SetHeaders(NetworkNodeSettings node, HttpClient client);
    }
}