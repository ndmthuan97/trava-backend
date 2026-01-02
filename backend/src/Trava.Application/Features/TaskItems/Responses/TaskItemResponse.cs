using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trava.Domain.Enums;

namespace Trava.Application.Features.TaskItems.Responses
{
    public class TaskItemResponse
    {
        public Guid Id { get; set; }
        public Guid SpaceId { get; set; }
        public Guid? ParentTaskId { get; set; } = null;
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string Priority { get; set; } = default!;
        public int Point { get; set; } = 1;
        public DateTimeOffset? StartDate { get; set; } = null;
        public DateTimeOffset? DueDate { get; set; } = null;
        public Guid? AssignedUserId { get; set; }
        public DateTimeOffset? AssignedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }
}