using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Trava.Application.Features.Spaces.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Trava.Application.Common.Exceptions;
using Trava.Shared.Enums;

namespace Trava.Application.Features.Spaces.Queries
{
    public record GetSpaceStatisticsQuery(Guid SpaceId) : IRequest<SpaceStatisticsResponse>;

    public class GetSpaceStatisticsQueryHandler : IRequestHandler<GetSpaceStatisticsQuery, SpaceStatisticsResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSpaceStatisticsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SpaceStatisticsResponse> Handle(GetSpaceStatisticsQuery request, CancellationToken cancellationToken)
        {
            var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();
            var taskRepo = _unitOfWork.GetRepository<TaskItem, Guid>();

            var space = await spaceRepo.GetByIdAsync(request.SpaceId);
            if (space == null) throw new AppException(CustomCode.SpaceNotFound);

            var tasks = await taskRepo.FindAsync(t => t.SpaceId == request.SpaceId, cancellationToken);

            var tasksByStatus = tasks
                .GroupBy(t => t.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Fill missing statuses with 0
            foreach (TaskItemStatus status in Enum.GetValues(typeof(TaskItemStatus)))
            {
                if (!tasksByStatus.ContainsKey(status))
                {
                    tasksByStatus[status] = 0;
                }
            }

            var tasksByPriority = tasks
                .GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key, g => g.Count());

            // Fill missing priorities with 0
            foreach (TaskItemPriority priority in Enum.GetValues(typeof(TaskItemPriority)))
            {
                if (!tasksByPriority.ContainsKey(priority))
                {
                    tasksByPriority[priority] = 0;
                }
            }

            return new SpaceStatisticsResponse
            {
                TotalTasks = tasks.Count,
                TasksByStatus = tasksByStatus,
                TasksByPriority = tasksByPriority
            };
        }
    }
}
