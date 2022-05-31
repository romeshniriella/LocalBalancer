using System.Collections.Generic;
using System.Net.Http;
using LocalBalancer.Tests.Framework;
using Polly.CircuitBreaker;

namespace LocalBalancer.Tests
{
    public partial class MultipleNodeTheories
    {
        private void MultiNodeRetryTest()
        {
            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveMultiNodeConfig
                    .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 3, circuitBreakThreshold: 2)),
                ExpectedExceptionType = typeof(HttpRequestException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    // try each node one by one
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress ), // at most one request to the node
                    new ExecutionResult(2, ConfigValues.SecondNodeAddress ), // first retry,
                    new ExecutionResult(3, ConfigValues.StandbyNodeAddress ), // second retry

                    // retries the primary once more.
                    new ExecutionResult(4, ConfigValues.PrimaryNodeAddress ), // third retry
                },
                ExpectedExceptionMessage = $"Too Many Requests: {ConfigValues.PrimaryNodeAddress}"
            }, "three nodes, executes each one, no circuit breaks: retryCount: 3, circuitBreakThreshold: 2");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveMultiNodeConfig
                    .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 3, circuitBreakThreshold: 10)),
                ExpectedExceptionType = typeof(HttpRequestException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    // try each node one by one
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress ), // at most one request to the node
                    new ExecutionResult(2, ConfigValues.SecondNodeAddress ), // first retry,
                    new ExecutionResult(3, ConfigValues.StandbyNodeAddress ), // second retry

                    // retries the primary once more.
                    new ExecutionResult(4, ConfigValues.PrimaryNodeAddress ), // third retry
                },
                ExpectedExceptionMessage = $"Too Many Requests: {ConfigValues.PrimaryNodeAddress}"
            }, "three nodes, executes each one, no circuit breaks: retryCount: 3, circuitBreakThreshold: 10");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveMultiNodeConfig
                    .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 6, circuitBreakThreshold: 10)),
                ExpectedExceptionType = typeof(HttpRequestException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress ), // at most one request to the node

                    // retries: keep switching through nodes
                    new ExecutionResult(2, ConfigValues.SecondNodeAddress ), // first retry,
                    new ExecutionResult(3, ConfigValues.StandbyNodeAddress ), // second retry
                    new ExecutionResult(4, ConfigValues.PrimaryNodeAddress ),
                    // -
                    new ExecutionResult(5, ConfigValues.SecondNodeAddress),
                    new ExecutionResult(6, ConfigValues.StandbyNodeAddress),
                    new ExecutionResult(7, ConfigValues.PrimaryNodeAddress ),
                },
                ExpectedExceptionMessage = $"Too Many Requests: {ConfigValues.PrimaryNodeAddress}"
            }, "three nodes, executes each one, no circuit breaks: retryCount: 6, circuitBreakThreshold: 10");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveMultiNodeConfig
                    .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 4, circuitBreakThreshold: 2)),
                ExpectedExceptionType = typeof(HttpRequestException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress ), // at most one request to the node

                    // retries
                    new ExecutionResult(2, ConfigValues.SecondNodeAddress ), // first retry,
                    new ExecutionResult(3, ConfigValues.StandbyNodeAddress ), // second retry
                    new ExecutionResult(4, ConfigValues.PrimaryNodeAddress ), // third retry
                    new ExecutionResult(5, ConfigValues.SecondNodeAddress) // fourth retry, throws from this second node.
                },
                ExpectedExceptionMessage = $"Too Many Requests: {ConfigValues.SecondNodeAddress}"
            }, "retryCount: 4, circuitBreakThreshold: 2");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveMultiNodeConfig
                    .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 3, circuitBreakThreshold: 1)),
                ExpectedExceptionType = typeof(BrokenCircuitException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress ),
                    new ExecutionResult(2, ConfigValues.SecondNodeAddress ),
                    new ExecutionResult(3, ConfigValues.StandbyNodeAddress ),
                },
                ExpectedExceptionMessage = "The circuit is now open and is not allowing calls."
            }, "retryCount: 3, circuitBreakThreshold: 1");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveMultiNodeConfig
                    .Merge(TestNodeConfigurationProvider.TrackingEnabledConfig)
                    .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 100, circuitBreakThreshold: 1)),
                ExpectedExceptionType = typeof(BrokenCircuitException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    // when tracking is enabled
                    // retry on three nodes
                    // when circuits starts to break, nodes should be set to disabled
                    // until none of the nodes are live
                    // then swithcer should throw NodeException
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress ),
                    new ExecutionResult(2, ConfigValues.SecondNodeAddress ),
                    new ExecutionResult(3, ConfigValues.StandbyNodeAddress ),
                },
                ExpectedExceptionMessage = "The circuit is now open and is not allowing calls."
            }, "retryCount: 100 times, circuitBreakThreshold: 1, it shouldn't retry 100 times");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveMultiNodeConfig
                    .Merge(TestNodeConfigurationProvider.TrackingEnabledConfig)
                    .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 100, circuitBreakThreshold: 2)),
                ExpectedExceptionType = typeof(BrokenCircuitException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    // when tracking is enabled
                    // retry on three nodes
                    // when circuits starts to break, nodes should be set to disabled
                    // until none of the nodes are live
                    // then swithcer should throw NodeException
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress ),
                    new ExecutionResult(2, ConfigValues.SecondNodeAddress ),
                    new ExecutionResult(3, ConfigValues.StandbyNodeAddress ),
                    new ExecutionResult(4, ConfigValues.PrimaryNodeAddress ),
                    new ExecutionResult(5, ConfigValues.SecondNodeAddress ),
                    new ExecutionResult(6, ConfigValues.StandbyNodeAddress ),
                },
                ExpectedExceptionMessage = "The circuit is now open and is not allowing calls."
            }, "retryCount: 100 times, circuitBreakThreshold: 2, it shouldn't retry 100 times");
        }
    }
}