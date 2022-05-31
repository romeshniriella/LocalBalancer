using System;
using Polly.CircuitBreaker;
using Polly.Registry;

namespace LocalBalancer
{
    internal static class Extensions
    {
        /// <summary>
        /// Get from policy registry if exists, otherwise adds a new one.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="key"></param>
        /// <param name="policyFactory"></param>
        /// <returns></returns>
        public static AsyncCircuitBreakerPolicy GetOrAdd(
            this IPolicyRegistry<string> registry,
            string key,
            Func<AsyncCircuitBreakerPolicy> policyFactory)
        {
            if (registry.TryGet(key, out AsyncCircuitBreakerPolicy policy))
            {
                return policy;
            }

            AsyncCircuitBreakerPolicy newPolicy = policyFactory();
            registry[key] = newPolicy;

            return newPolicy;
        }
    }
}