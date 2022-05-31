using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocalBalancer.Http
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the defailt <see cref="HttpRestClient"/> balancer component which returns a <see cref="HttpResponseMessage"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddBalancedHttpRestApiClient(
            this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddTransient<IRestApiClient, HttpRestClient>()
            .AddBalancer<HttpResponseMessage, BalancedHttpClientProvider, RequestHeadersProvider, DefaultMetricsTracker>(configuration); ;
        }
    }
}