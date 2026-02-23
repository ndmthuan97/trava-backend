using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Shared.Enums;
using Trava.Domain.Enums;
using Trava.Application.Interfaces.Services;

namespace Trava.Application.Features.TaskItems.Commands
{
    public record StatusTaskItemCommand([property: JsonIgnore] Guid Id, TaskItemStatus Status, [property: JsonIgnore] Guid CompletedBy) : IRequest;

    public class StatusTaskItemCommandHandler : IRequestHandler<StatusTaskItemCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubNotificationService _hubNotificationService;

        public StatusTaskItemCommandHandler(IUnitOfWork unitOfWork, IHubNotificationService hubNotificationService)
        {
            _unitOfWork = unitOfWork;
            _hubNotificationService = hubNotificationService;
        }

        public async Task Handle(StatusTaskItemCommand request, CancellationToken cancellationToken)
        {
            var taskItemRepo = _unitOfWork.GetRepository<TaskItem, Guid>();
            var taskItem = await taskItemRepo.GetByIdAsync(request.Id) ?? throw new AppException(CustomCode.TaskItemNotFound);

            if (taskItem.AssignedUserId != request.CompletedBy)
            {
                throw new AppException(CustomCode.UnauthorizedAction);
            }

            taskItem.Status = request.Status;
            taskItemRepo.Update(taskItem);
            await _unitOfWork.CommitAsync();

            if (request.Status == TaskItemStatus.Completed)
            {
                var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();
                var space = await spaceRepo.GetByIdAsync(taskItem.SpaceId);

                if (space != null)
                {
                    Guid ownerId = Guid.Empty;
                    if (space.SpaceType == SpaceType.Personal)
                    {
                        ownerId = space.CreatedBy;
                    }
                    else
                    {
                        var spaceMemberRepo = _unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();
                        var ownerMember = await spaceMemberRepo.FirstOrDefaultAsync(sm => sm.SpaceId == space.Id && sm.Role == SpaceRole.Owner);
                        if (ownerMember != null)
                        {
                            ownerId = ownerMember.UserId;
                        }
                        else
                        {
                            return; 
                        }
                    }

                    if (ownerId != request.CompletedBy)
                    {
                        var userRepo = _unitOfWork.GetRepository<User, Guid>();
                        var user = await userRepo.GetByIdAsync(request.CompletedBy);
                        var completedByEmail = user?.Email ?? "a user";

                        await _hubNotificationService.SendNotificationToUserAsync(
                           ownerId,
                           "TaskCompleted",
                           new 
                           { 
                               TaskId = taskItem.Id, 
                               Title = taskItem.Title,
                               Message = $"Task {taskItem.Title} has been completed by {completedByEmail}"
                           });
                    }
                }
            }
        }
    }

    public class StatusTaskItemCommandValidator : AbstractValidator<StatusTaskItemCommand>
    {
        public StatusTaskItemCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Task item ID is required.");
            RuleFor(x => x.CompletedBy).NotEmpty().WithMessage("CompletedBy user ID is required.");
        }
    }
}