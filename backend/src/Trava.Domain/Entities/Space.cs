using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Domain.Common;
using Trava.Domain.Enums;

namespace Trava.Domain.Entities
{
    public class Space : BaseTimeEntity<Guid>
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public SpaceType SpaceType { get; set; } = SpaceType.Personal;

        // Navigation properties
        public virtual User User { get; set; } = default!;
        public virtual ICollection<SpaceMember> Members { get; set; } = new List<SpaceMember>();
        public virtual ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
    }
}