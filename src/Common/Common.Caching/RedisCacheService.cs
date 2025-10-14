using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly DistributedCacheEntryOptions _defaultOptions;

        public RedisCacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            _defaultOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var cachedData = await _distributedCache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedData))
                return default;

            return JsonSerializer.Deserialize<T>(cachedData);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = expiration.HasValue
                ? new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration }
                : _defaultOptions;

            var serializedData = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, serializedData, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            var cachedData = await _distributedCache.GetStringAsync(key);
            return !string.IsNullOrEmpty(cachedData);
        }
    }
}
