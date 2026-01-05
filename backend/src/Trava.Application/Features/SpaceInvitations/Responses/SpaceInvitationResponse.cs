using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Domain.Enums;

namespace Trava.Application.Features.SpaceInvitations.Responses
{
    public class SpaceInvitationResponse
    {
        public Guid Id { get; set; }
        public Guid SpaceId { get; set; }
        public Guid InvitedUserId { get; set; }
        public string Role { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateTime? ExpiredAt { get; set; }
    }
}