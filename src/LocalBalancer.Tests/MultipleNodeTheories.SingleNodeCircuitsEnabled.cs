using System.Collections.Generic;
using System.Net.Http;
using LocalBalancer.Tests.Framework;
using Polly.CircuitBreaker;

namespace LocalBalancer.Tests
{
    public partial class MultipleNodeTheories
    {
        private void SingleNodeCircuitsEnabled()
        {
            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveSingleNodeConfig
                        .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 0, circuitBreakThreshold: 1)),
                ExpectedExceptionType = typeof(HttpRequestException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress) // at most one request to the node
                },
                ExpectedExceptionMessage = $"Too Many Requests: {ConfigValues.PrimaryNodeAddress}"
            }, "SingleNodeCircuitsEnabled: at most one request to the node");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveSingleNodeConfig
                        .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 1, circuitBreakThreshold: 1)),
                ExpectedExceptionType = typeof(BrokenCircuitException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress) // at most one request to the node
                },
                ExpectedExceptionMessage = "The circuit is now open and is not allowing calls."
            }, "SingleNodeCircuitsEnabled: at most one request to the node, circuit breaks");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveSingleNodeConfig
                        .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 1, circuitBreakThreshold: 2)),
                ExpectedExceptionType = typeof(HttpRequestException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress), // at most one request to the node,
                    new ExecutionResult(2, ConfigValues.PrimaryNodeAddress) // first retry
                },
                ExpectedExceptionMessage = $"Too Many Requests: {ConfigValues.PrimaryNodeAddress}"
            }, "SingleNodeCircuitsEnabled: two requests, one retry");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveSingleNodeConfig
                        .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 2, circuitBreakThreshold: 2)),
                ExpectedExceptionType = typeof(BrokenCircuitException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress), // at most one request to the node
                    new ExecutionResult(2, ConfigValues.PrimaryNodeAddress) // first retry, circuit is broken, no more calls to server
                },
                ExpectedExceptionMessage = "The circuit is now open and is not allowing calls."
            }, "SingleNodeCircuitsEnabled: two requests, one retry, circuit is broken, no more calls to server");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveSingleNodeConfig
                        .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 3, circuitBreakThreshold: 2)),
                ExpectedExceptionType = typeof(BrokenCircuitException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress), // at most one request to the node
                    new ExecutionResult(2, ConfigValues.PrimaryNodeAddress), // first retry, circuit is broken @ 2, only one node
                },
                ExpectedExceptionMessage = "The circuit is now open and is not allowing calls."
            }, "SingleNodeCircuitsEnabled: two requests, one retry, circuit is broken @ 2, only one node");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveSingleNodeConfig
                    .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 10, circuitBreakThreshold: 2)),
                ExpectedExceptionType = typeof(BrokenCircuitException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress), // at most one request to the node
                    new ExecutionResult(2, ConfigValues.PrimaryNodeAddress), // first retry, circuit is broken @ 2, only one node
                },
                ExpectedExceptionMessage = "The circuit is now open and is not allowing calls."
            }, "SingleNodeCircuitsEnabled: 10 retries, circuit is broken @ 2, only one node");
        }
    }
}