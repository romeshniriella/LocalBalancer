using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using LocalBalancer.Abstractions;

namespace LocalBalancer.Tests.Framework
{
    public class FailedTestRetryClient : TestRetryClient
    {
        public FailedTestRetryClient(IBalancedNetworkPolicy<List<ExecutionResult>> nodePolicy) : base(nodePolicy)
        {
        }

        protected override Task<List<ExecutionResult>> GetTestResultsAsync(Uri nodeAddress)
        {
            base.GetTestResultsAsync(nodeAddress);

            throw new HttpRequestException($"Too Many Requests: {nodeAddress}");
        }
    }
}