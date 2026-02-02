using System;
using System.Threading.Tasks;

namespace Trava.Application.Interfaces.Services
{
    public interface ITokenRegistryService
    {
        Task SaveRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiry);
        Task<string?> GetRefreshTokenAsync(string userId);
        Task RevokeRefreshTokenAsync(string userId);
    }
}
