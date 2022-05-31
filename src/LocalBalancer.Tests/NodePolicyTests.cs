using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LocalBalancer.Tests.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LocalBalancer.Tests
{
    public class NodePolicyTests
    {
        public class When_There_Is_Only_One_Configured_Node
        {
            public class And_The_Node_Is_Down
            {
                [Theory, ClassData(typeof(MultipleNodeTheories))]
                public async Task It_Throws_Broken_Circuit_Exception(MultipleNodeTheoryData theoryData, string test)
                {
                    // Arrange
                    IConfiguration configuration = new ConfigurationBuilder()
                        .AddInMemoryCollection(theoryData.CustomSettings)
                        .Build();

                    IServiceProvider sp = new ServiceCollection()
                        .AddLogging()
                        .AddBalancer<List<ExecutionResult>>(configuration)
                        .AddSingleton<ITestRetryClient, FailedTestRetryClient>()
                        .AddSingleton<IMetricsTracker, DefaultMetricsTracker>()
                        .BuildServiceProvider(true);

                    var retryTestClient = (FailedTestRetryClient)sp.GetRequiredService<ITestRetryClient>();

                    // Act
                    Exception thrownException = await Assert.ThrowsAsync(
                        exceptionType: theoryData.ExpectedExceptionType,
                        () => retryTestClient.GetResults(default));

                    // Assert
                    thrownException.Message.Should().Be(theoryData.ExpectedExceptionMessage, test);

                    retryTestClient
                        .TestResults
                        .Should()
                        .BeEquivalentTo(theoryData.ExpectedTestResults, test);
                }

                [Theory, ClassData(typeof(MultipleNodeDualRequestTrackingTheories))]
                public async Task It_Throws_When_Tracking_On_For_Multiple_Calls(MultipleNodeTheoryData theoryData, string test)
                {
                    // Arrange
                    IConfiguration configuration = new ConfigurationBuilder()
                        .AddInMemoryCollection(theoryData.CustomSettings)
                        .Build();

                    IServiceProvider sp = new ServiceCollection()
                        .AddLogging()
                        .AddBalancer<List<ExecutionResult>>(configuration)
                        .AddSingleton<ITestRetryClient, FailedTestRetryClient>()
                        .AddSingleton<IMetricsTracker, DefaultMetricsTracker>()
                        .BuildServiceProvider(true);

                    var retryTestClient = (FailedTestRetryClient)sp.GetRequiredService<ITestRetryClient>();

                    // Act
                    await Assert.ThrowsAsync(
                          exceptionType: theoryData.ExpectedExceptionType,
                          () => retryTestClient.GetResults(default));

                    await Assert.ThrowsAsync(
                          exceptionType: theoryData.ExpectedExceptionType,
                          () => retryTestClient.GetResults(default));

                    Exception thrownException = await Assert.ThrowsAsync(
                        exceptionType: theoryData.ExpectedExceptionType,
                        () => retryTestClient.GetResults(default));

                    // Assert
                    thrownException.Message.Should().Be(theoryData.ExpectedExceptionMessage, test);

                    retryTestClient
                        .TestResults
                        .Should()
                        .BeEquivalentTo(theoryData.ExpectedTestResults, test);
                }
            }

            public class And_The_Node_Is_Up
            {
                /// <summary>
                /// Scenario:
                /// If the nodes are working fine, we should get a result right away through the retry policy.
                /// </summary>
                /// <returns></returns>
                [Fact]
                public async Task The_Request_Is_Successful()
                {
                    // Arrange
                    Dictionary<string, string> configData = TestNodeConfigurationProvider
                        .ActiveSingleNodeConfig
                        .Merge(TestNodeConfigurationProvider.GetRetryConfig(3, 2));

                    IConfiguration configuration = new ConfigurationBuilder()
                        .AddInMemoryCollection(configData)
                        .Build();

                    IServiceProvider sp = new ServiceCollection()
                        .AddLogging()
                        .AddBalancer<List<ExecutionResult>>(configuration)
                        .AddSingleton<ITestRetryClient, SuccessfulTestRetryClient>()
                        .AddSingleton<IMetricsTracker, DefaultMetricsTracker>()
                        .BuildServiceProvider(true);

                    ITestRetryClient retryTestClient = sp.GetRequiredService<ITestRetryClient>();

                    // Act
                    List<ExecutionResult> retryResult = await retryTestClient.GetResults(default);

                    // Assert
                    retryResult.Should().BeEquivalentTo(new List<ExecutionResult>
                    {
                        new ExecutionResult(1, ConfigValues.PrimaryNodeAddress )
                    });
                }
            }
        }
    }
}