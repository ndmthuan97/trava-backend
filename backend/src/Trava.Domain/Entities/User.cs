using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Domain.Common;
using Trava.Domain.Constants;
using Trava.Domain.Enums;

namespace Trava.Domain.Entities
{
    public class User : BaseEntity<Guid>
    {
        public string? FullName { get; set; }
        public string Email { get; set; } = default!;
        public string AvatarUrl { get; set; } = AppConstants.DEFAULT_AVATAR;
        public Role Role { get; set; } = Role.User;
        public string? Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
        public string Password { get; set; } = default!;
        public string? RefreshToken { get; set; }
        public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? LastLoginAt { get; set; } = null;
        public DateTimeOffset? LastModifiedAt { get; set; } = null;

        //Navigation properties
        public virtual ICollection<Space> Spaces { get; set; } = new List<Space>();
        public virtual ICollection<SpaceMember> SpaceMembers { get; set; } = new List<SpaceMember>();
        public virtual ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    }
}