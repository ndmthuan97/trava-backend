using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Domain.Common;
using Trava.Domain.Enums;

namespace Trava.Domain.Entities
{
    public class TaskItem : BaseTimeEntity<Guid>
    {
        public Guid SpaceId { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public TaskItemStatus Status { get; set; } = TaskItemStatus.NotStart;
        public TaskItemPriority Priority { get; set; } = TaskItemPriority.Low;
        public int Point { get; set; } = 1;
        public DateTimeOffset? StartDate { get; set; } = null;
        public DateTimeOffset? DueDate { get; set; } = null;
        public Guid? AssignedUserId { get; set; }
        public DateTimeOffset? AssignedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }

        // Navigation properties
        public virtual Space Space { get; set; } = default!;
        public virtual User? AssignedUser { get; set; } = null;
        public virtual User Creator { get; set; } = default!;
        public virtual ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    }
}