using System.Net;

namespace LocalBalancer.Http
{
    public sealed class ComponentSettings
    {
        public static readonly HttpStatusCode[] TransientErrors =
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.ServiceUnavailable
        };
    }
}