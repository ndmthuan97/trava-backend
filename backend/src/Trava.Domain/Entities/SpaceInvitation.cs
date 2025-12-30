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
        public Guid? InvitedUserId { get; set; }
        public string? InvitedEmail { get; set; }
        public Role Role { get; set; } = Role.Member;
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
        public Guid InvitedBy { get; set; }
        public DateTime? ExpiredAt { get; set; }

        // Navigation properties
        public Space Space { get; set; } = default!;
        public User? InvitedUser { get; set; }
        public User Inviter { get; set; } = default!;
    }
}