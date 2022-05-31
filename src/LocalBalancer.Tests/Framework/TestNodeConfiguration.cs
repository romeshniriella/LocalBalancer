using System.Collections.Generic;

namespace LocalBalancer.Tests.Framework
{
    internal class ConfigValues
    {
        public const string PrimaryNodeAddress = "https://bitcoin-primary.address/";
        public const string SecondNodeAddress = "https://bitcoin-backup.address/";
        public const string StandbyNodeAddress = "https://bitcoin-standby.address/";
        public const string True = "true";
    }

    internal class TestNodeConfigurationProvider
    {
        public static Dictionary<string, string> ActiveMultiNodeConfig => new()
        {
            { "BalancerConfiguration:Nodes:Primary:NetworkAddress", ConfigValues.PrimaryNodeAddress },
            { "BalancerConfiguration:Nodes:Primary:IsActive", ConfigValues.True },
            { "BalancerConfiguration:Nodes:Second:NetworkAddress", ConfigValues.SecondNodeAddress },
            { "BalancerConfiguration:Nodes:Second:IsActive", ConfigValues.True },
            { "BalancerConfiguration:Nodes:Standby:NetworkAddress", ConfigValues.StandbyNodeAddress },
            { "BalancerConfiguration:Nodes:Standby:IsActive", ConfigValues.True },
        };

        public static Dictionary<string, string> ActiveSingleNodeConfig => new()
        {
            { "BalancerConfiguration:Nodes:Primary:NetworkAddress", ConfigValues.PrimaryNodeAddress },
            { "BalancerConfiguration:Nodes:Primary:IsActive", ConfigValues.True },
        };

        public static Dictionary<string, string> TrackingEnabledConfig => new()
        {
            { "BalancerConfiguration:IsTrackingEnabled", "true" },
        };

        public static Dictionary<string, string> GetRetryConfig(int retryCount, int circuitBreakThreshold) => new()
        {
            { "BalancerConfiguration:RetryCount", retryCount.ToString() },
            { "BalancerConfiguration:CircuitBreakThreadhold", circuitBreakThreshold.ToString() },
        };
    }
}