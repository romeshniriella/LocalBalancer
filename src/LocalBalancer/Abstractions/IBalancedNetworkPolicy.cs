using System;
using System.Threading;
using System.Threading.Tasks;

namespace LocalBalancer.Abstractions
{
    public interface IBalancedNetworkPolicy<TResult>
    {
        Task<TResult> ExecuteAsync(Func<NetworkNodeSettings, Task<TResult>> action, string requestIdentifier, CancellationToken cancellationToken);

        Task<TResult> ExecuteOnceAsync(Func<NetworkNodeSettings, Task<TResult>> action, string requestIdentifier);
    }
}