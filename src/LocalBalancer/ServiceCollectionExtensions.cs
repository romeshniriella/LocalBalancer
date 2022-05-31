using LocalBalancer.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocalBalancer
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a <see cref="BalancedHttpClientProvider"/> and <see cref="RequestHeadersProvider"/> and <seealso cref="DefaultMetricsTracker"/>
        /// with the <see cref="IHttpClientBuilder"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddBalancer<THttpResult>(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddBalancer<THttpResult, BalancedHttpClientProvider, RequestHeadersProvider, DefaultMetricsTracker>(
                configuration);
        }

        /// <summary>
        /// Registers a <see cref="BalancedHttpClientProvider"/> and an implementation of <see cref="IRequestHeadersProvider"/>
        /// with the <see cref="IHttpClientBuilder"/>
        /// </summary>
        /// <typeparam name="THeaderProvider"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddBalancer<THttpResult, THeaderProvider, TMetricsProvider>(
            this IServiceCollection services,
            IConfiguration configuration)
            where THeaderProvider : class, IRequestHeadersProvider
            where TMetricsProvider : class, IMetricsTracker
        {
            return services.AddBalancer<THttpResult, BalancedHttpClientProvider, THeaderProvider, TMetricsProvider>(
                configuration);
        }

        /// <summary>
        /// Registers an implementation of <see cref="IBalancedHttpClientProvider"/> and an implementation of <see cref="IRequestHeadersProvider"/>
        /// with the <see cref="IHttpClientBuilder"/>.
        /// </summary>
        /// <typeparam name="TClientProvider"></typeparam>
        /// <typeparam name="THeaderProvider"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddBalancer<THttpResult, TClientProvider, THeaderProvider, TMetricsProvider>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TClientProvider : class, IBalancedHttpClientProvider
            where THeaderProvider : class, IRequestHeadersProvider
            where TMetricsProvider : class, IMetricsTracker
        {
            services
                .AddOptions()
                .AddMemoryCache()
                .AddHttpClient()
                .Configure<BalancerConfiguration>(configuration.GetSection(nameof(BalancerConfiguration)))
                .AddTransient<IBalancerConfigSource, DefaultBalancerConfigSource>()
                .AddTransient<ICacheHelper, CacheHelper>()
                .AddTransient<IMetricsTracker, TMetricsProvider>()
                .AddTransient<IBalancer, Balancer>()
                .AddTransient<IBalancedNetworkPolicy<THttpResult>, BalancedNetworkPolicy<THttpResult>>()
                .AddTransient<IRequestHeadersProvider, THeaderProvider>()
                .AddTransient<IBalancedHttpClientProvider, TClientProvider>()
                .AddPolicyRegistry();

            // whatever module requires support to seamless retries on multiple node configurations,
            // they can inject a IMultiNodeHttpClient into their services
            return services;
        }
    }
}