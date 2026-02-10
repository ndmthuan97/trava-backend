using System;
using Trava.Domain.Enums;

namespace Trava.Application.Features.SpaceInvitations.Responses
{
    public record SpaceInvitationResponse
    {
        public Guid Id { get; init; }
        public Guid SpaceId { get; init; }
        public Guid InvitedUserId { get; init; }
        public SpaceRole Role { get; init; }
        public InvitationStatus Status { get; init; }
        public DateTime? ExpiredAt { get; init; }
    }
}