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

            // 1. Total Users
            var totalUsers = await userRepo.CountAsync(u => true, cancellationToken);

            // 2. Total Spaces
            var totalSpaces = await spaceRepo.CountAsync(s => true, cancellationToken);

            // 3. Returning Users within last week (logged in at least once in the last 7 days)
            var oneWeekAgo = DateTimeOffset.UtcNow.AddDays(-7);
            var returningUsers = await userRepo.CountAsync(u => u.LastLoginAt >= oneWeekAgo, cancellationToken);

            return new StatisticsResponse
            {
                TotalUsers = totalUsers,
                TotalSpaces = totalSpaces,
                ReturningUsersLastWeek = returningUsers
            };
        }
    }
}
