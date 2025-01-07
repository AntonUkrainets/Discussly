using Discussly.Server.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Discussly.Server.Data.Repositories
{
    public class CacheRepository(IDistributedCache distributedCache) : ICacheRepository
    {
        public async Task AddOrUpdateAsync(string key, string value, CancellationToken cancellationToken)
        {
            await distributedCache.SetStringAsync(key, value, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }, cancellationToken);
            await distributedCache.RefreshAsync(key, cancellationToken);
        }

        public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken)
        {
            return await distributedCache.GetStringAsync(key, cancellationToken);
        }
    }
}