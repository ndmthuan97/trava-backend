using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Trava.Application.Features.Dashboards.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Dashboards.Queries
{
    public record GetStatisticsQuery() : IRequest<StatisticsResponse>;

    public class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQuery, StatisticsResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStatisticsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<StatisticsResponse> Handle(GetStatisticsQuery request, CancellationToken cancellationToken)
        {
            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();
            var taskRepo = _unitOfWork.GetRepository<TaskItem, Guid>();

            var now = DateTimeOffset.UtcNow;
            var oneWeekAgo = now.AddDays(-7);
            var twoWeeksAgo = now.AddDays(-14);

            // 1. Users
            var totalUsersNow = await userRepo.CountAsync(u => true, cancellationToken);
            var totalUsersPrev = await userRepo.CountAsync(u => u.CreatedAt <= oneWeekAgo, cancellationToken);
            var totalUsersTwoWeeksAgo = await userRepo.CountAsync(u => u.CreatedAt <= twoWeeksAgo, cancellationToken);

            // 2. Spaces
            var totalSpacesNow = await spaceRepo.CountAsync(s => true, cancellationToken);
            var totalSpacesPrev = await spaceRepo.CountAsync(s => s.CreatedAt <= oneWeekAgo, cancellationToken);
            var totalSpacesTwoWeeksAgo = await spaceRepo.CountAsync(s => s.CreatedAt <= twoWeeksAgo, cancellationToken);

            // 3. Tasks
            var totalTasksNow = await taskRepo.CountAsync(t => true, cancellationToken);
            var totalTasksPrev = await taskRepo.CountAsync(t => t.CreatedAt <= oneWeekAgo, cancellationToken);
            var totalTasksTwoWeeksAgo = await taskRepo.CountAsync(t => t.CreatedAt <= twoWeeksAgo, cancellationToken);

            // 4. Returning Users (logged in within the period)
            var returningUsersThisWeek = await userRepo.CountAsync(u => u.LastLoginAt >= oneWeekAgo, cancellationToken);
            var returningUsersLastWeek = await userRepo.CountAsync(u => u.LastLoginAt >= twoWeeksAgo && u.LastLoginAt < oneWeekAgo, cancellationToken);

            return new StatisticsResponse
            {
                TotalUsers = totalUsersNow,
                TotalUsersTwoWeeksAgo = totalUsersTwoWeeksAgo,
                UserGrowth = CalculateGrowth(totalUsersNow, totalUsersPrev),
                TotalSpaces = totalSpacesNow,
                TotalSpacesTwoWeeksAgo = totalSpacesTwoWeeksAgo,
                SpaceGrowth = CalculateGrowth(totalSpacesNow, totalSpacesPrev),
                TotalTasks = totalTasksNow,
                TotalTasksTwoWeeksAgo = totalTasksTwoWeeksAgo,
                TaskGrowth = CalculateGrowth(totalTasksNow, totalTasksPrev),
                ReturningUsers = returningUsersThisWeek,
                ReturningUserRate = totalUsersNow > 0 ? Math.Round((double)returningUsersThisWeek / totalUsersNow * 100, 2) : 0
            };
        }

        private double CalculateGrowth(int current, int previous)
        {
            if (previous == 0) return current * 100; // e.g. from 0 to 1 is 100% growth
            return Math.Round((double)(current - previous) / previous * 100, 2);
        }
    }
}
