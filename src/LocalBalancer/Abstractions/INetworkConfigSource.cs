using Microsoft.Extensions.Options;

namespace LocalBalancer.Abstractions
{
    /// <summary>
    /// Contract of the <see cref="BalancerConfiguration"/> source.
    /// Extensions could grab the config from anywhere they like.
    /// the <seealso cref="DefaultBalancerConfigSource"/> pulls in from <see cref="IOptions{TOptions}"/>
    /// </summary>
    public interface IBalancerConfigSource
    {
        BalancerConfiguration GetBalancerConfiguration();
    }
}