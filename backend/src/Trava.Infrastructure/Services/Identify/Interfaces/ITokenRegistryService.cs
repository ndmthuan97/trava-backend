using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trava.Infrastructure.Services.Identify.Interfaces
{
    public interface ITokenRegistryService
    {
        Task SaveRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiry);
        Task<string?> GetRefreshTokenAsync(string userId);
        Task RevokeRefreshTokenAsync(string userId);
    }
}
