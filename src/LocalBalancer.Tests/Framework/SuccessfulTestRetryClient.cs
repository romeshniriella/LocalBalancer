using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalBalancer.Abstractions;

namespace LocalBalancer.Tests.Framework
{
    public class SuccessfulTestRetryClient : TestRetryClient
    {
        public SuccessfulTestRetryClient(IBalancedNetworkPolicy<List<ExecutionResult>> nodePolicy) : base(nodePolicy)
        {
        }

        protected override Task<List<ExecutionResult>> GetTestResultsAsync(Uri nodeAddress)
        {
            base.GetTestResultsAsync(nodeAddress);

            return Task.FromResult(TestResults);
        }
    }
}