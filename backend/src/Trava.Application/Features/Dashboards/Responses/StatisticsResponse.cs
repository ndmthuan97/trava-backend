using System;

namespace Trava.Application.Features.Dashboards.Responses
{
    public record StatisticsResponse
    {
        public int TotalUsers { get; init; }
        public int TotalUsersTwoWeeksAgo { get; init; }
        public double UserGrowth { get; init; }
        public int TotalSpaces { get; init; }
        public int TotalSpacesTwoWeeksAgo { get; init; }
        public double SpaceGrowth { get; init; }
        public int TotalTasks { get; init; }
        public int TotalTasksTwoWeeksAgo { get; init; }
        public double TaskGrowth { get; init; }
        public int ReturningUsers { get; init; }
        public double ReturningUserRate { get; init; }
    }
}
