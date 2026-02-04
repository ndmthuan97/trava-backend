using System;

namespace Trava.Application.Features.SpaceInvitations.Responses
{
    public record SpaceInvitationResponse
    {
        public Guid Id { get; init; }
        public Guid SpaceId { get; init; }
        public Guid InvitedUserId { get; init; }
        public string Role { get; init; } = default!;
        public string Status { get; init; } = default!;
        public DateTime? ExpiredAt { get; init; }
    }
}