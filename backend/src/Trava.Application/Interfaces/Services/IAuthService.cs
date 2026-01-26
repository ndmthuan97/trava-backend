using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Application.Features.Auth.DTOs;
using Trava.Shared.Enums;

namespace Trava.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<CustomCode> RegisterAsync(RegisterRequestDto request);
        Task<(CustomCode, AuthResultDto)> LoginAsync(LoginRequestDto request);
        Task LogoutAsync(string userId, string accessToken);
        Task<CustomCode> ChangePasswordAsync(ChangePasswordRequestDto request);
        Task<(CustomCode, AuthResultDto)> RefreshTokenAsync(RefreshTokenRequestDto request);
    }
}