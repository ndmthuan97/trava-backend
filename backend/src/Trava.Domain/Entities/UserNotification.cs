using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trava.Domain.Entities
{
    public class UserNotification
    {
        public Guid TargetUserId { get; set; }
        public Guid NotificationId { get; set; }
        public bool IsRead { get; set; }

        // Navigation properties
        public virtual User TargetUser { get; set; } = default!;
        public virtual Notification Notification { get; set; } = default!;
    }
}