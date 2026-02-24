using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Domain.Common;
using Trava.Domain.Enums;

namespace Trava.Domain.Entities
{
    public class SpaceInvitation : BaseEntity<Guid>
    {
        public Guid SpaceId { get; set; }
        public Guid InvitedUserId { get; set; }
        public SpaceRole SpaceRole { get; set; } = SpaceRole.Member;
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;


        // Navigation properties
        public Space Space { get; set; } = default!;
        public User InvitedUser { get; set; } = default!;
    }
}