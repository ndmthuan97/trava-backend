using System;

namespace Trava.Application.Features.Dashboards.Responses
{
    public record StatisticsResponse
    {
        public int TotalUsers { get; init; }
        public int TotalSpaces { get; init; }
        public int ReturningUsersLastWeek { get; init; }
    }
}
