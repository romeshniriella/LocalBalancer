using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Xunit;

namespace LocalBalancer.Tests
{
    public class NodeConfigurationTests
    {
        private readonly string SampleConfigJsonFile = $"{nameof(BalancerConfiguration)}.json";

        private readonly BalancerConfiguration SampleNodeConfiguration = new BalancerConfiguration()
        {
            Nodes =
                {
                    {
                        "PublicNet", new NetworkNodeSettings
                        {
                            Priority = 1,
                            IsActive = true,
                            NetworkAddress = new Uri("http://scooterlabs.com/echo"),
                            Headers = new Dictionary<string, string>
                            {
                                { "X-API-KEY" , Guid.NewGuid().ToString("N") }
                            }
                        }
                    },
                    {
                        "PublicNetBackup", new NetworkNodeSettings()
                        {
                            Priority = 2,
                            IsActive = true,
                            NetworkAddress = new Uri("https://dead/")
                        }
                    },
                    {
                        "PrivateMain", new NetworkNodeSettings
                        {
                            Priority = 3,
                            IsActive = true,
                            NetworkAddress = new Uri("http://scooterlabs.com/echo"),
                            Headers = new Dictionary<string, string>
                            {
                                { "X-API-KEY" , Guid.NewGuid().ToString("N") },
                                { "X-AUTH-KEY" , Guid.NewGuid().ToString("N") }
                            }
                        }
                    },
                    {
                        "PrivateBackup",new NetworkNodeSettings()
                        {
                            Priority = 4,
                            IsActive = true,
                            NetworkAddress = new Uri("https://dead-too/")
                        }
                    }
                }
        };

        [Fact]
        public void ConvertToJsonAndSave()
        {
            string configJson = JsonConvert.SerializeObject(new
            {
                NodeConfiguration = SampleNodeConfiguration
            }, Formatting.Indented);

            File.WriteAllText(SampleConfigJsonFile, configJson);
        }

        [Fact(Skip = "run only when necessary")]
        public void ServiceCollectionTest()
        {
            // Make sure we are always using the latest config schema
            this.ConvertToJsonAndSave();

            string currentDir = Directory.GetCurrentDirectory(); // resolves to the tests\Flare.WalletService.IntegrationTests\bin\Debug\net5.0
            var settingsFileProvider = new PhysicalFileProvider(currentDir);

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(settingsFileProvider, path: SampleConfigJsonFile, optional: false, reloadOnChange: false)
                .Build();

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddOptions()
                .AddBalancer<HttpRequestMessage>(configuration)
                .BuildServiceProvider(true);

            IOptions<BalancerConfiguration> nodeConfigOptions = serviceProvider.GetRequiredService<IOptions<BalancerConfiguration>>();

            nodeConfigOptions.Value.Should().BeEquivalentTo(SampleNodeConfiguration);
        }
    }
}