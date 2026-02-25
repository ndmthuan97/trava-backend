using System.Collections.Generic;
using Trava.Domain.Enums;

namespace Trava.Application.Features.Spaces.Responses
{
    public record SpaceStatisticsResponse
    {
        public int TotalTasks { get; init; }
        public Dictionary<TaskItemStatus, int> TasksByStatus { get; init; } = new();
        public Dictionary<TaskItemPriority, int> TasksByPriority { get; init; } = new();
    }
}
