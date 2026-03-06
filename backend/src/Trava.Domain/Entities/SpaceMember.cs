using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Domain.Enums;

namespace Trava.Domain.Entities
{
    public class SpaceMember
    {
        public Guid SpaceId { get; set; }
        public Guid UserId { get; set; }
        public SpaceRole SpaceRole { get; set; } = SpaceRole.Member;
        public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

        public Guid? DeletedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        // Navigation properties
        public virtual Space Space { get; set; } = default!;
        public virtual User User { get; set; } = default!;
    }
}