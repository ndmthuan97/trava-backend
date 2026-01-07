using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Trava.Infrastructure.Services.Identify.Interfaces;

namespace Trava.Infrastructure.Services.Identify
{
    public class TokenBlackListService : ITokenBlackListService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<TokenBlackListService> _logger;

        public TokenBlackListService(IDistributedCache distributedCache, ILogger<TokenBlackListService> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<bool> AreUserTokensInvalidatedAsync(string userId, DateTime tokenIssuedAt)
        {
            try
            {
                var keyToChecks = new [] { $"user_tokens_invalidated_{userId}", $"user_tokens_invalidated_except_{userId}" };
                foreach (var key in keyToChecks)
                {
                    var invalidationData = await _distributedCache.GetStringAsync(key);
                    if (string.IsNullOrEmpty(invalidationData)) continue;
                    if (long.TryParse(invalidationData, out var invalidationTime))
                    {
                        var invalidationDateTime = DateTimeOffset.FromUnixTimeSeconds(invalidationTime).UtcDateTime;
                        if (tokenIssuedAt < invalidationDateTime) return true;
                    } 
                    else
                    {
                        _logger.LogWarning("Invalid data format in cache for key {Key}: {InvalidationData}", key, invalidationData);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user tokens are invalidated");
                return false;
            }
        }

        // Blacklist toàn bộ token của user
        public async Task BlacklistAllUserTokensAsync(string userId)
        {
            var invalidationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var userTokenKey = $"user_tokens_exception_{userId}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };
            await _distributedCache.SetStringAsync(userTokenKey, invalidationTime.ToString(), options);
            await _distributedCache.RemoveAsync($"user_tokens_exception_{userId}");
            _logger.LogInformation("All tokens for user {UserId} have been blacklisted at {InvalidationTime}", userId, DateTimeOffset.FromUnixTimeSeconds(invalidationTime));
        }

        // Blacklist toàn bộ token trừ token hiện tại (Logout all devices trừ thiết bị hiện tại)
        public async Task BlacklistAllUserTokensExceptAsync(string userId, string currentToken)
        {
            if (string.IsNullOrWhiteSpace(currentToken))
            {
                _logger.LogWarning("Current token is null or empty. Defaulting to full blacklist.");
                await BlacklistAllUserTokensAsync(userId);
                return;
            }

            var invalidationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var exceptInvalidateKey = $"user_tokens_invalidated_except_{userId}";
            var exceptionTokenKey = $"user_tokens_exception_{userId}";

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };
            
            await _distributedCache.SetStringAsync(exceptInvalidateKey, invalidationTime.ToString(), options);
            await _distributedCache.SetStringAsync(exceptionTokenKey, currentToken, options);

            _logger.LogInformation("Blacklisted all tokens for user {UserId} except the current token (at {Time})", userId, DateTimeOffset.FromUnixTimeSeconds(invalidationTime));
        }

        // Blacklist 1 token cụ thể
        public async Task BlacklistTokenAsync(string token, DateTime expiry)
        {
            var key = $"blacklist:{token}";
            var ttl = (expiry - DateTime.UtcNow).TotalSeconds;
            if (ttl > 0)
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttl)
                };

                await _distributedCache.SetStringAsync(key, "blacklisted", options);
            }
            _logger.LogInformation("Token blacklisted until {ExpirationDate}", expiry);
        }

        // Lấy token ngoại lệ (Logout tất cả session trừ session hiện tại)
        public async Task<string?> GetExceptionTokenAsync(string userId)
        {
            var key = $"user_tokens_exception_{userId}";
            return await _distributedCache.GetStringAsync(key);
        }

        // Kiểm tra Token có bị blacklist hay không
        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            try
            {
                var tokenData = await _distributedCache.GetStringAsync($"blacklist:{token}");
                return !string.IsNullOrEmpty(tokenData);
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if token is blacklisted");
                return false;
            }
        }
    }
}