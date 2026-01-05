using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Trava.Application.Common.Exceptions;
using Trava.Application.Features.Auth.DTOs;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Services;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.Infrastructure.Services.Identify
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtHandler _jwtHandler;
        private readonly IServiceProvider _serviceProvider;

        public AuthService(
            IUnitOfWork unitOfWork,
            ILogger<AuthService> logger,
            JwtHandler jwtHandler,
            IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _jwtHandler = jwtHandler;
            _serviceProvider = serviceProvider;
        }

        public async Task<(CustomCode, AuthResultDto)> LoginAsync(LoginRequestDto request)
        {
            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            var user = await userRepo.FirstOrDefaultAsync(u => u.Email == request.Email) ?? throw new AppException(CustomCode.UserNotExists);

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Attempt to login with unconfirmed email: {Email}.", request.Email);
                throw new AppException(CustomCode.UserNotConfirmed);
            }

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
            var claims = TokenHelper.GetClaims(user, new List<Claim>());

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

            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtHandler.GetExpiryInSecond(),
                Requires2FA = false,
                Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? user.Email
            };
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
                EmailConfirmed = true,
            };

            await userRepo.AddAsync(newUser);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("New user registered with email: {Email}.", request.Email);
            return CustomCode.Success;
        }
    }
}