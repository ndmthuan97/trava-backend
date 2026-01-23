using System;
using Trava.Domain.Enums;

namespace Trava.Application.Features.Users.Responses
{
    public record UserResponse(
        Guid Id,
        string FullName,
        string Email,
        string AvatarUrl,
        string Role,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? LastLoginAt
    );
}
