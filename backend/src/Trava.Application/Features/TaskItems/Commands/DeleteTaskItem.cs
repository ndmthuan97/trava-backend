using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.Application.Features.TaskItems.Commands
{
    public record DeleteTaskItemCommand(Guid Id, Guid UserId) : IRequest<Unit>;

    public class DeleteTaskItemCommandHandler : IRequestHandler<DeleteTaskItemCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTaskItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteTaskItemCommand request, CancellationToken cancellationToken)
        {
            var taskItemRepo = _unitOfWork.GetRepository<TaskItem, Guid>();
            var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();
            var spaceMemberRepo = _unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();

            var taskItem = await taskItemRepo.GetByIdAsync(request.Id);
            if (taskItem == null)
            {
                throw new AppException(CustomCode.TaskItemNotFound);
            }

            var space = await spaceRepo.GetByIdAsync(taskItem.SpaceId);
            if (space == null)
            {
                throw new AppException(CustomCode.SpaceNotFound);
            }

            if (space.SpaceType == SpaceType.Personal)
            {
                if (space.CreatedBy != request.UserId)
                {
                    throw new AppException(CustomCode.UnauthorizedAction);
                }
            }
            else
            {
                var isOwner = await spaceMemberRepo.ExistsAsync(sm =>
                    sm.SpaceId == taskItem.SpaceId &&
                    sm.UserId == request.UserId &&
                    sm.Role == SpaceRole.Owner);

                if (!isOwner)
                {
                    throw new AppException(CustomCode.UnauthorizedAction);
                }
            }

            taskItemRepo.Remove(taskItem);
            await _unitOfWork.CommitAsync();

            return Unit.Value;
        }
    }
}
