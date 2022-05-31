using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LocalBalancer.Tests.Framework
{
    public interface ITestRetryClient
    {
        Task<List<ExecutionResult>> GetResults(CancellationToken cancellationToken);
    }
}