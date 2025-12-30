using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Domain.Common;

namespace Trava.Domain.Entities
{
    public class TaskComment : BaseTimeEntity<Guid>
    {
        public Guid TaskItemId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = default!;
        // Navigation properties
        public virtual TaskItem TaskItem { get; set; } = default!;
        public virtual User User { get; set; } = default!;
    }
}