using MediatR;
using Microsoft.Extensions.Logging;
using Trava.Application.Common.Exceptions;
using Trava.Application.Common.Helpers;
using Trava.Application.Features.Auth.Responses;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Services;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Shared.Enums;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using FluentValidation;

namespace Trava.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<(CustomCode, AuthResponse)>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, (CustomCode, AuthResponse)>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;
    private readonly IJwtService _jwtService;
    private readonly ITokenRegistryService _tokenRegistryService;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenCommandHandler> logger,
        IJwtService jwtService,
        ITokenRegistryService tokenRegistryService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _jwtService = jwtService;
        _tokenRegistryService = tokenRegistryService;
    }

    public async Task<(CustomCode, AuthResponse)> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new AppException(CustomCode.InvalidToken);
        if (!Guid.TryParse(userIdStr, out var userId)) throw new AppException(CustomCode.InvalidToken);

        var userRepo = _unitOfWork.GetRepository<User, Guid>();
        var user = await userRepo.GetByIdAsync(userId);

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

    private async Task<AuthResponse> GenerateToken(User user, bool populateExp)
    {
        var userClaims = new List<Claim>();
        var claims = TokenHelper.GetClaims(user, userClaims);

        var tokenOptions = _jwtService.GenerateTokenOptions(claims);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        var refreshToken = TokenHelper.GenerateRefreshToken();
        user.RefreshToken = refreshToken;

        if (populateExp)
        {
            user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(30);
            user.LastLoginAt = DateTimeOffset.UtcNow;
        }

        var userRepo = _unitOfWork.GetRepository<User, Guid>();
        userRepo.Update(user);
        await _unitOfWork.CommitAsync();

        await _tokenRegistryService.SaveRefreshTokenAsync(user.Id.ToString(), refreshToken, TimeSpan.FromDays(30));

        return new AuthResponse(
            accessToken,
            refreshToken,
            _jwtService.GetExpiryInSecond(),
            claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? user.Email
        );
    }
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
