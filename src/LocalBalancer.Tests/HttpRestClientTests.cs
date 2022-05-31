using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using LocalBalancer.Http;
using LocalBalancer.Tests.Framework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LocalBalancer.Tests
{
    public class HttpRestClientTests
    {
        public const string RequestUri = "http://scooterlabs.com/echo?hello";

        public class When_The_Request_Is_Not_Skipped
        {
            [Theory, ClassData(typeof(HttpRestClientTheories))]
            public async Task It_Retries_When_Nodes_Are_Down(MultipleNodeTheoryData theoryData, string test)
            {
                // Arrange
                var logger = new LoggerStub<HttpRestClient>();

                IConfiguration configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(theoryData.CustomSettings)
                    .Build();

                IServiceProvider sp = new ServiceCollection()
                    .AddBalancedHttpRestApiClient(configuration)
                    .AddSingleton(typeof(ILogger<>), typeof(LoggerStub<>))
                    .AddSingleton<ILogger<HttpRestClient>>(logger)
                    .AddSingleton<IMetricsTracker, DefaultMetricsTracker>()
                    .BuildServiceProvider(true);

                IRestApiClient restClient = sp.GetRequiredService<IRestApiClient>();
                IMemoryCache memoryCache = sp.GetRequiredService<IMemoryCache>();

                // Act
                HttpResponseMessage response =
                    await restClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, RequestUri), default);

                // Assert
                memoryCache.Get<int>("NodeSelector.CurrentNode.Index")
                    .Should()
                    .Be(theoryData.ExpectedCurrentNodeIndex, test);
            }
        }

        internal class HttpRestClientTheories : TheoryData<MultipleNodeTheoryData, string>
        {
            public HttpRestClientTheories()
            {
                this.Add(new MultipleNodeTheoryData
                {
                    // Retry three times on each node,
                    // we also have circuit breakers protecting the nodes
                    // the requests retries but since there's a circuit breaker protecting the primary node and it's open not allowing any connections
                    // it throws a broken circuit error
                    CustomSettings = TestNodeConfigurationProvider
                        .ActiveMultiNodeConfig
                        .Merge(TestNodeConfigurationProvider.GetRetryConfig(retryCount: 3, circuitBreakThreshold: 1)),
                    ExpectedCurrentNodeIndex = 0
                }, "no retries, just logs");
            }
        }
    }
}