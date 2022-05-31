using System.Collections.Generic;
using System.Net.Http;
using LocalBalancer.Tests.Framework;

namespace LocalBalancer.Tests
{
    public partial class MultipleNodeTheories
    {
        private void SingleNodeRetryTests()
        {
            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveSingleNodeConfig
                        .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 0, int.MaxValue)),
                ExpectedExceptionType = typeof(HttpRequestException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress ) // at most one request to the node
                },
                ExpectedExceptionMessage = $"Too Many Requests: {ConfigValues.PrimaryNodeAddress}",
            }, "at most one request to the node");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveSingleNodeConfig
                        .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 1, int.MaxValue)),
                ExpectedExceptionType = typeof(HttpRequestException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress ),// first execution
                    new ExecutionResult(2, ConfigValues.PrimaryNodeAddress ) // first retry, two requests to the node
                },
                ExpectedExceptionMessage = $"Too Many Requests: {ConfigValues.PrimaryNodeAddress}"
            }, "first retry, two requests to the node");

            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveSingleNodeConfig
                        .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 2, int.MaxValue)),
                ExpectedExceptionType = typeof(HttpRequestException),
                ExpectedTestResults = new List<ExecutionResult>
                {
                    new ExecutionResult(1, ConfigValues.PrimaryNodeAddress ),// first execution
                    new ExecutionResult(2, ConfigValues.PrimaryNodeAddress ),// first retry, two requests to the node
                    new ExecutionResult(3, ConfigValues.PrimaryNodeAddress ) // second retry, three requests to the node
                },
                ExpectedExceptionMessage = $"Too Many Requests: {ConfigValues.PrimaryNodeAddress}"
            }, "second retry, three requests to the node");

            // if you were to set the circuit breaker allowance to a higher number
            // and keep the retry count to any number
            // the policy will retry for that amount of times
            for (int attempt = 4; attempt <= 10; attempt++)
            {
                var results = new List<ExecutionResult>();

                for (int executions = 1; executions <= attempt + 1; executions++)
                {
                    results.Add(new ExecutionResult(executions, ConfigValues.PrimaryNodeAddress));
                }

                this.Add(new MultipleNodeTheoryData
                {
                    CustomSettings = TestNodeConfigurationProvider
                        .ActiveSingleNodeConfig
                        .Merge(TestNodeConfigurationProvider.GetRetryConfig(attempt, int.MaxValue)),
                    ExpectedExceptionType = typeof(HttpRequestException),
                    ExpectedTestResults = results,
                    ExpectedExceptionMessage = $"Too Many Requests: {ConfigValues.PrimaryNodeAddress}"
                }, $"{attempt} retry, {attempt + 1} requests to the node");
            }
        }
    }
}