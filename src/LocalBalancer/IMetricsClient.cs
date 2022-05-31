using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LocalBalancer
{
    /// <summary>
    /// Track metrics
    /// </summary>
    public interface IMetricsTracker
    {
        Task TrackRequestError(NetworkNodeSettings networkNodeSettings, Exception exception, string requestIdentifier);

        Task<TResult> TrackRequestTiming<TResult>(NetworkNodeSettings currentNode, Func<NetworkNodeSettings, Task<TResult>> action, string requestIdentifier);
    }

    /// <summary>
    /// Logs the metrics by default
    /// </summary>
    public class DefaultMetricsTracker : IMetricsTracker
    {
        private ILogger<DefaultMetricsTracker> _logger;

        public DefaultMetricsTracker(ILogger<DefaultMetricsTracker> logger)
        {
            _logger = logger;
        }

        public Task TrackRequestError(NetworkNodeSettings networkNodeSettings, Exception exception, string requestIdentifier)
        {
            _logger.LogError(exception, "Balancer:RequestError:{@node} {id}", networkNodeSettings, requestIdentifier);

            return Task.CompletedTask;
        }

        public async Task<TResult> TrackRequestTiming<TResult>(NetworkNodeSettings currentNode, Func<NetworkNodeSettings, Task<TResult>> action, string requestIdentifier)
        {
            var latencyTimer = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Balancer:RequestCount:{@node} {id} [1]", currentNode, requestIdentifier);

                return await action(currentNode);
            }
            finally
            {
                latencyTimer.Stop();

                _logger.LogInformation("Balancer:RequestTiming:{@node} {id} {time}ms", currentNode, requestIdentifier, latencyTimer.Elapsed.TotalMilliseconds);
            }
        }
    }
}