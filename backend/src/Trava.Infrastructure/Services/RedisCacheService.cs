using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Trava.Application.Interfaces.Services;

namespace Trava.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _cache.GetStringAsync(key);
            return value is null ? default : JsonSerializer.Deserialize<T>(value);
        }

        public Task RemoveAsync(string key) => _cache.RemoveAsync(key);

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null)
        {
            var json = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(30)
            };
            await _cache.SetStringAsync(key, json, options);
        }
    }
}