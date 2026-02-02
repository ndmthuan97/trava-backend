using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Services;
using Trava.Domain.Entities;
using Trava.Shared.Enums;
using System.Text.Json.Serialization;

namespace Trava.Application.Features.Auth.Commands;

public record LogoutCommand(
    [property: JsonIgnore] string UserId,
    [property: JsonIgnore] string AccessToken
) : IRequest;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenRegistryService _tokenRegistryService;

    public LogoutCommandHandler(IUnitOfWork unitOfWork, ITokenRegistryService tokenRegistryService)
    {
        _unitOfWork = unitOfWork;
        _tokenRegistryService = tokenRegistryService;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userGuid)) throw new UnauthorizedAccessException("Invalid user id");

        var userRepo = _unitOfWork.GetRepository<User, Guid>();
        var user = await userRepo.GetByIdAsync(userGuid);
        if (user != null)
        {
            user.RefreshToken = null!;
            user.RefreshTokenExpiryTime = null;
            userRepo.Update(user);
        }

        await _tokenRegistryService.RevokeRefreshTokenAsync(request.UserId);

        await _unitOfWork.CommitAsync();
    }
}