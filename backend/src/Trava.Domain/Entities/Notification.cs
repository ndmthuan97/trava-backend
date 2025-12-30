using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Domain.Common;

namespace Trava.Domain.Entities
{
    public class Notification : BaseEntity<Guid>
    {
        public string Type { get; set; } = default!;
        public string Payload { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        public virtual ICollection<UserNotification> UserNotifications { get; set; } = [];
    }
}