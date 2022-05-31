using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LocalBalancer.Abstractions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;

namespace LocalBalancer
{
    public class BalancedNetworkPolicy<TResult> : IBalancedNetworkPolicy<TResult>
    {
        private readonly ILogger<BalancedNetworkPolicy<TResult>> _logger;
        private readonly IMetricsTracker _metricsClient;
        private readonly IBalancer _balancer;
        private readonly IPolicyRegistry<string> _policyRegistry;

        public BalancedNetworkPolicy(IBalancer balancer, IPolicyRegistry<string> policyRegistry, IMetricsTracker metricsClient, ILogger<BalancedNetworkPolicy<TResult>> logger)
        {
            _balancer = balancer;
            _policyRegistry = policyRegistry;
            _metricsClient = metricsClient;
            _logger = logger;
        }

        public Task<TResult> ExecuteAsync(Func<NetworkNodeSettings, Task<TResult>> action, string requestIdentifier, CancellationToken cancellationToken)
        {
            _balancer.Reset();

            return this.CreateMultipleNodeRetryPolicy(requestIdentifier)
                .ExecuteAsync(async (context, token) =>
                {
                    NetworkNodeSettings currentNode = _balancer.GetCurrentNodeSettings();

                    string key = $"BREAKER:{currentNode.NetworkAddress}";

                    AsyncCircuitBreakerPolicy breaker = _policyRegistry.GetOrAdd(key, this.CircuitBreakerFactory);

                    _logger.LogInformation("Using Current Node {@NodeConfig} with Breaker {BreakerId} to execute the {TResult}", currentNode, key, typeof(TResult).Name);

                    return await breaker.ExecuteAsync(() => _metricsClient.TrackRequestTiming(currentNode, action, requestIdentifier));
                }, new Context(), cancellationToken);
        }

        public async Task<TResult> ExecuteOnceAsync(Func<NetworkNodeSettings, Task<TResult>> action, string requestIdentifier)
        {
            NetworkNodeSettings currentNode = _balancer.GetCurrentNodeSettings();

            try
            {
                return await _metricsClient.TrackRequestTiming(currentNode, action, requestIdentifier);
            }
            catch (Exception exception)
            {
                await _metricsClient.TrackRequestError(_balancer.GetCurrentNodeSettings(), exception, requestIdentifier);

                throw;
            }
        }

        private AsyncCircuitBreakerPolicy CircuitBreakerFactory()
                => // Break the circuit after the specified number of consecutive exceptions
                   // and keep circuit broken for the specified duration.
                    Policy
                        .Handle<HttpRequestException>()
                        .CircuitBreakerAsync(
                            exceptionsAllowedBeforeBreaking: _balancer.CircuitBreakThreadhold,
                            durationOfBreak: TimeSpan.FromMinutes(_balancer.CircuitBreakDurationMinutes),
                            onBreak: (ex, t) => _balancer.SetTrackingState(_balancer.GetCurrentNodeSettings(), TrackingState.Offline),
                            onReset: () => _balancer.SetTrackingState(_balancer.GetCurrentNodeSettings(), TrackingState.Online));

        private AsyncRetryPolicy CreateMultipleNodeRetryPolicy(string requestIdentifier)
            => Policy
                   .Handle<HttpRequestException>()
                   .Or<BrokenCircuitException>(e =>
                   {
                       return _balancer.ShouldBreak();
                   })
                   .RetryAsync(
                       retryCount: _balancer.RetryCount,
                       onRetry: async (exception, retryCount, context) =>
                       {
                           await _metricsClient.TrackRequestError(_balancer.GetCurrentNodeSettings(), exception, requestIdentifier);

                           _logger.LogInformation(exception, "Retry attempt {retryCount}. Moving to next node", retryCount);

                           // move to the next node.
                           _balancer.MoveToNextNode();
                       });
    }
}