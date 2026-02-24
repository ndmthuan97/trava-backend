using System;
using Trava.Domain.Enums;

namespace Trava.Application.Features.SpaceInvitations.Responses
{
    public record SpaceInvitationResponse
    {
        public Guid Id { get; init; }
        public Guid SpaceId { get; init; }
        public string SpaceName { get; init; } = default!;
        public SpaceType SpaceType { get; init; }
        public Guid InvitedUserId { get; init; }
        public SpaceRole SpaceRole { get; init; } // Fixed naming to match entity: SpaceRole
        public InvitationStatus Status { get; init; }
        public DateTime? ExpiredAt { get; init; }
    }
}