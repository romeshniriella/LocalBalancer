using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace LocalBalancer
{
    /// <summary>
    /// Wraps the memory cache and adds logging around the behavior
    /// </summary>
    internal sealed class CacheHelper : ICacheHelper
    {
        private readonly ILogger<CacheHelper> _logger;
        private readonly IMemoryCache _memoryCache;

        public CacheHelper(IMemoryCache memoryCache, ILogger<CacheHelper> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public int GetCurrentNodeIndex()
        {
            try
            {
                if (_memoryCache.TryGetValue(Constants.CurrentNodeIndexCacheKey, out int index))
                {
                    return index;
                }
                else
                {
                    _logger.LogWarning("Could not retrieve the current node index from cache");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve the current node index due to {error}", ex.Message);
            }

            return 0;
        }

        public void SetCurrentNodeIndex(int nextNodeIndex)
        {
            try
            {
                _memoryCache.Set(Constants.CurrentNodeIndexCacheKey, nextNodeIndex);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not cache the current node index due to {error}", ex.Message);
            }
        }
    }
}