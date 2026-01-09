using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trava.Infrastructure.Services.Identify.Interfaces
{
    public interface ITokenRegistryService
    {
        Task RegisterTokenAsync(string userId, string token, TimeSpan expiry);
        Task<bool> IsTokenAllowedAsync(string userId, string token);
        Task InvalidateTokenAsync(string userId, string token);
        Task InvalidateAllTokensAsync(string userId);
        Task InvalidateOtherTokensAsync(string userId, string currentToken);
    }
}
