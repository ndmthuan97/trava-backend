using MediatR;
using Microsoft.Extensions.Logging;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Services;
using Trava.Domain.Entities;
using Trava.Shared.Enums;
using FluentValidation;
using System.Text.Json.Serialization;

namespace Trava.Application.Features.Auth.Commands;

public record ChangePasswordCommand(
    [property: JsonIgnore] Guid UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword,
    [property: JsonIgnore] string CurrentAccessToken
) : IRequest<CustomCode>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, CustomCode>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;
    private readonly ITokenRegistryService _tokenRegistryService;

    public ChangePasswordCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ChangePasswordCommandHandler> logger,
        ITokenRegistryService tokenRegistryService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _tokenRegistryService = tokenRegistryService;
    }

    public async Task<CustomCode> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userRepo = _unitOfWork.GetRepository<User, Guid>();
        var user = await userRepo.GetByIdAsync(request.UserId) ?? throw new AppException(CustomCode.UserNotExists);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password)) throw new AppException(CustomCode.InvalidCredentials);

        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.Password)) throw new AppException(CustomCode.NewPasswordSameAsOld);

        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        userRepo.Update(user);
        await _unitOfWork.CommitAsync();

        await _tokenRegistryService.RevokeRefreshTokenAsync(request.UserId.ToString());
        _logger.LogInformation("RefreshToken revoked for user {UserId} after password change", request.UserId);

        return CustomCode.Success;
    }
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current Password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm Password is required.")
            .Equal(x => x.NewPassword).WithMessage("The new password and confirmation password do not match.");

    }
}
