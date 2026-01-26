using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Trava.Application.Common.Exceptions;
using Trava.Application.Features.Auth.DTOs;
using Trava.Application.Features.Auth.Enums;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Services;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Infrastructure.Services.Identify.Interfaces;
using Trava.Shared.Enums;

namespace Trava.Infrastructure.Services.Identify
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtHandler _jwtHandler;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITokenRegistryService _tokenRegistryService;

        public AuthService(
            IUnitOfWork unitOfWork,
            ILogger<AuthService> logger,
            JwtHandler jwtHandler,
            IServiceProvider serviceProvider,
            ITokenRegistryService tokenRegistryService
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _jwtHandler = jwtHandler;
            _serviceProvider = serviceProvider;
            _tokenRegistryService = tokenRegistryService;
        }

        public async Task<(CustomCode, AuthResultDto)> LoginAsync(LoginRequestDto request)
        {
            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            var user = await userRepo.FirstOrDefaultAsync(u => u.Email == request.Email) ?? throw new AppException(CustomCode.UserNotExists);

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                _logger.LogWarning("Invalid password attempt for email: {Email}.", request.Email);
                throw new AppException(CustomCode.InvalidCredentials);
            }

            if (user.Status == UserStatus.Inactive)
            {
                _logger.LogWarning("Attempt to login to locked account: {Email}.", request.Email);
                throw new AppException(CustomCode.UserAccountLocked);
            }

            user.LastLoginAt = DateTimeOffset.UtcNow;

            var authResponse = await GenerateToken(user, populateExp: true);
            return (CustomCode.Success, authResponse);
        }

        private async Task<AuthResultDto> GenerateToken(User user, bool populateExp)
        {
            var userClaims = new List<Claim>();
            var claims = TokenHelper.GetClaims(user, userClaims);

            var tokenOptions = _jwtHandler.GenerateTokenOptions(claims);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            var refreshToken = TokenHelper.GenerateRefreshToken();
            user.RefreshToken = refreshToken;

            if (populateExp)
            {
                user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(30);
                user.LastLoginAt = DateTimeOffset.UtcNow;
            }

            _unitOfWork.GetRepository<User, Guid>().Update(user);
            await _unitOfWork.CommitAsync();

            await _tokenRegistryService.SaveRefreshTokenAsync(user.Id.ToString(), refreshToken, TimeSpan.FromDays(30));

            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtHandler.GetExpiryInSecond(),
                Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? user.Email
            };
        }

        public async Task<(CustomCode, AuthResultDto)> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var principal = _jwtHandler.GetPrincipalFromExpiredToken(request.AccessToken);
            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new AppException(CustomCode.InvalidToken);
            if (!Guid.TryParse(userIdStr, out var userId)) throw new AppException(CustomCode.InvalidToken);

            var user = await _unitOfWork.GetRepository<User, Guid>().GetByIdAsync(userId);

            if (user == null || user.RefreshToken != request.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
                throw new AppException(CustomCode.InvalidToken, new[] { "Refresh token is invalid or expired." });

            var storedRefreshToken = await _tokenRegistryService.GetRefreshTokenAsync(userIdStr);
            if (storedRefreshToken != request.RefreshToken)
            {
                _logger.LogWarning("Refresh token mismatch for user {UserId}. Session might have been revoked or taken over.", userIdStr);
                throw new AppException(CustomCode.InvalidToken, new[] { "Session expired or invalid." });
            }

            if (user.Status != UserStatus.Active)
            {
                _logger.LogWarning("Attempt to login with locked account for email: {Email}.", user.Email);
                throw new AppException(CustomCode.UserAccountLocked);
            }

            var authRespone = await GenerateToken(user, populateExp: false);
            return (CustomCode.Success, authRespone);
        }

        public async Task<CustomCode> RegisterAsync(RegisterRequestDto request)
        {
            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            var existingUser = await userRepo.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Attempt to register with existing email: {Email}.", request.Email);
                throw new AppException(CustomCode.EmailAlreadyExists);
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FullName = request.FullName,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            };

            await userRepo.AddAsync(newUser);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("New user registered with email: {Email}.", request.Email);
            return CustomCode.Success;
        }

        public async Task LogoutAsync(string userId, string accessToken)
        {
            if (!Guid.TryParse(userId, out var userGuid)) throw new UnauthorizedAccessException("Invalid user id");

            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            var user = await userRepo.GetByIdAsync(userGuid);
            if (user != null)
            {
                user.RefreshToken = null!;
                user.RefreshTokenExpiryTime = null;
                userRepo.Update(user);
            }

            await _tokenRegistryService.RevokeRefreshTokenAsync(userId);

            await _unitOfWork.CommitAsync();
        }

        public async Task<CustomCode> ChangePasswordAsync(ChangePasswordRequestDto request)
        {
            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            var user = await userRepo.GetByIdAsync(request.UserId) ?? throw new AppException(CustomCode.UserNotExists);

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password)) throw new AppException(CustomCode.InvalidCredentials);

            if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.Password)) throw new AppException(CustomCode.NewPasswordSameAsOld);

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            if (request.LogoutBehavior == LogoutBehavior.LogoutAllIncludingCurrent)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
            }

            userRepo.Update(user);
            await _unitOfWork.CommitAsync();

            await _tokenRegistryService.RevokeRefreshTokenAsync(request.UserId.ToString());
            _logger.LogInformation("RefreshToken revoked for user {UserId} after password change", request.UserId);


            return CustomCode.Success;
        }
    }
}