using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LocalBalancer.Abstractions;

namespace LocalBalancer.Tests.Framework
{
    public abstract class TestRetryClient : ITestRetryClient
    {
        public readonly List<ExecutionResult> TestResults;
        private readonly IBalancedNetworkPolicy<List<ExecutionResult>> _nodePolicy;

        public TestRetryClient(IBalancedNetworkPolicy<List<ExecutionResult>> nodePolicy)
        {
            _nodePolicy = nodePolicy;
            TestResults = new List<ExecutionResult>();
        }

        public Task<List<ExecutionResult>> GetResults(CancellationToken cancellationToken)
        {
            return _nodePolicy.ExecuteAsync(
                   // which will create a new rpc client every time we do a retry
                   node => this.GetTestResultsAsync(node.NetworkAddress),
                   "Test-GetResults",
            // : provides cancellation capabilities to the policy
            cancellationToken);
        }

        protected virtual Task<List<ExecutionResult>> GetTestResultsAsync(Uri nodeAddress)
        {
            TestResults.Add(new ExecutionResult(TestResults.Count + 1, nodeAddress));

            return Task.FromResult(TestResults);
        }
    }
}