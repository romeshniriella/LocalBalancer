using System.Collections.Generic;
using LocalBalancer.Tests.Framework;
using Polly.CircuitBreaker;
using Xunit;

namespace LocalBalancer.Tests
{
    public partial class MultipleNodeDualRequestTrackingTheories : TheoryData<MultipleNodeTheoryData, string>
    {
        public MultipleNodeDualRequestTrackingTheories()
        {
            this.Add(new MultipleNodeTheoryData
            {
                CustomSettings = TestNodeConfigurationProvider
                    .ActiveMultiNodeConfig
                    .Merge(TestNodeConfigurationProvider.TrackingEnabledConfig)
                    .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 3, circuitBreakThreshold: 1)),
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
            }, "multiple calls to same endpoint: retryCount: 3 times, circuitBreakThreshold: 1, traking on");
        }
    }
}