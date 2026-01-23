using System;

namespace Trava.Application.Features.TaskItems.Responses
{
    public record TaskItemResponse(
        Guid Id,
        Guid SpaceId,
        Guid? ParentTaskId,
        string Title,
        string Description,
        string Status,
        string Priority,
        int Point,
        DateTimeOffset? StartDate,
        DateTimeOffset? DueDate,
        Guid? AssignedUserId,
        DateTimeOffset? AssignedAt,
        DateTimeOffset? CompletedAt
    );
}