using MediatR;
using Microsoft.Extensions.Logging;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Services;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Shared.Enums;
using Trava.Application.Common.Helpers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Trava.Application.Features.Auth.Responses;
using FluentValidation;

namespace Trava.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<(CustomCode, AuthResponse)>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, (CustomCode, AuthResponse)>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly IJwtService _jwtService;
    private readonly ITokenRegistryService _tokenRegistryService;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<LoginCommandHandler> logger,
        IJwtService jwtService,
        ITokenRegistryService tokenRegistryService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _jwtService = jwtService;
        _tokenRegistryService = tokenRegistryService;
    }

    public async Task<(CustomCode, AuthResponse)> Handle(LoginCommand request, CancellationToken cancellationToken)
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

        var authResponse = await GenerateToken(user, populateExp: true);
        return (CustomCode.Success, authResponse);
    }

    private async Task<AuthResponse> GenerateToken(User user, bool populateExp)
    {
        var userClaims = new List<System.Security.Claims.Claim>();
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

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtService.GetExpiryInSecond(),
            Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? user.Email
        };
    }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
