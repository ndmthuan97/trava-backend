using System;

namespace Trava.Application.Features.Users.Responses
{
    public record UserSearchResponse
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string? AvatarUrl { get; init; }
    }
}
