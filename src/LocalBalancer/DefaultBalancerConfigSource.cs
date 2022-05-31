using LocalBalancer.Abstractions;
using Microsoft.Extensions.Options;

namespace LocalBalancer
{
    public class DefaultBalancerConfigSource : IBalancerConfigSource
    {
        private readonly BalancerConfiguration _nodeConfiguration;

        public DefaultBalancerConfigSource(IOptions<BalancerConfiguration> nodeConfigOptions)
        {
            _nodeConfiguration = nodeConfigOptions.Value;
        }

        public BalancerConfiguration GetBalancerConfiguration()
        {
            return _nodeConfiguration;
        }
    }
}