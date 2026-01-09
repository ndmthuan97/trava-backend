using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Trava.Infrastructure.Services.Identify.Interfaces;

namespace Trava.Infrastructure.Services.Identify
{
    public class TokenRegistryService : ITokenRegistryService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<TokenRegistryService> _logger;
        private const int MaxTokensPerUser = 3;

        public TokenRegistryService(IDistributedCache distributedCache, ILogger<TokenRegistryService> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        private string GetCacheKey(string userId) => $"user_tokens:{userId}";

        public async Task RegisterTokenAsync(string userId, string token, TimeSpan expiry)
        {
            var key = GetCacheKey(userId);
            var tokens = await GetTokensAsync(userId);

            tokens.Insert(0, token);

            if (tokens.Count > MaxTokensPerUser)
            {
                tokens = tokens.Take(MaxTokensPerUser).ToList();
            }

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30) // Long enough to cover refresh token expiry
            };

            await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(tokens), options);
            _logger.LogInformation("Token registered for user {UserId}. Current active tokens: {Count}", userId, tokens.Count);
        }

        public async Task<bool> IsTokenAllowedAsync(string userId, string token)
        {
            var tokens = await GetTokensAsync(userId);
            return tokens.Contains(token);
        }

        public async Task InvalidateTokenAsync(string userId, string token)
        {
            var key = GetCacheKey(userId);
            var tokens = await GetTokensAsync(userId);

            if (tokens.Remove(token))
            {
                await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(tokens));
                _logger.LogInformation("Token invalidated for user {UserId}", userId);
            }
        }

        public async Task InvalidateAllTokensAsync(string userId)
        {
            await _distributedCache.RemoveAsync(GetCacheKey(userId));
            _logger.LogInformation("All tokens invalidated for user {UserId}", userId);
        }

        public async Task InvalidateOtherTokensAsync(string userId, string currentToken)
        {
            var key = GetCacheKey(userId);
            var tokens = new List<string> { currentToken };
            await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(tokens));
            _logger.LogInformation("All tokens except current invalidated for user {UserId}", userId);
        }

        private async Task<List<string>> GetTokensAsync(string userId)
        {
            var data = await _distributedCache.GetStringAsync(GetCacheKey(userId));
            if (string.IsNullOrEmpty(data))
            {
                return new List<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(data) ?? new List<string>();
            }
            catch (JsonException)
            {
                _logger.LogWarning("Failed to deserialize tokens for user {UserId}", userId);
                return new List<string>();
            }
        }
    }
}
