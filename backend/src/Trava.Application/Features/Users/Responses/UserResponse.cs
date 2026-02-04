using System;
using Trava.Domain.Enums;

namespace Trava.Application.Features.Users.Responses
{
    public record UserResponse
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string AvatarUrl { get; init; } = default!;
        public string Role { get; init; } = default!;
        public string Status { get; init; } = default!;
        public string? Phone { get; init; }
        public DateTime? BirthDate { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? LastLoginAt { get; init; }
    }
}
