using System;

namespace Trava.Application.Features.TaskItems.Responses
{
    public record TaskItemResponse
    {
        public Guid Id { get; init; }
        public Guid SpaceId { get; init; }
        public string Title { get; init; } = default!;
        public string Description { get; init; } = default!;
        public string Status { get; init; } = default!;
        public string Priority { get; init; } = default!;
        public int Point { get; init; }
        public DateTimeOffset? StartDate { get; init; }
        public DateTimeOffset? DueDate { get; init; }
        public Guid? AssignedUserId { get; init; }
        public DateTimeOffset? AssignedAt { get; init; }
        public DateTimeOffset? CompletedAt { get; init; }
    }
}