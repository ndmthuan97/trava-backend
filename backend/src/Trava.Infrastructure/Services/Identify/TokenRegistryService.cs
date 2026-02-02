using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Trava.Application.Interfaces.Services;

namespace Trava.Infrastructure.Services.Identify
{
    public class TokenRegistryService : ITokenRegistryService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<TokenRegistryService> _logger;

        public TokenRegistryService(IDistributedCache distributedCache, ILogger<TokenRegistryService> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        private string GetCacheKey(string userId) => $"user_refresh:{userId}";

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiry)
        {
            var key = GetCacheKey(userId);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            };

            await _distributedCache.SetStringAsync(key, refreshToken, options);
            _logger.LogInformation("RefreshToken saved for user {UserId}. Previous session invalidated.", userId);
        }

        public async Task<string?> GetRefreshTokenAsync(string userId)
        {
            return await _distributedCache.GetStringAsync(GetCacheKey(userId));
        }

        public async Task RevokeRefreshTokenAsync(string userId)
        {
            await _distributedCache.RemoveAsync(GetCacheKey(userId));
            _logger.LogInformation("RefreshToken revoked for user {UserId}", userId);
        }
    }
}
