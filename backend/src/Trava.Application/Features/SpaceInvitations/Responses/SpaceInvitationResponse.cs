using System;

namespace Trava.Application.Features.SpaceInvitations.Responses
{
    public record SpaceInvitationResponse(
        Guid Id,
        Guid SpaceId,
        Guid InvitedUserId,
        string Role,
        string Status,
        DateTime? ExpiredAt
    );
}