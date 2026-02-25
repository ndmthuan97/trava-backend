using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Services;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Shared.Enums;
using FluentValidation;

namespace Trava.Application.Features.TaskItems.Commands
{
    public record QuickUpdateTaskItemCommand(
        [property: JsonIgnore] Guid Id,
        TaskItemStatus? Status,
        DateTimeOffset? StartDate,
        DateTimeOffset? DueDate,
        int? Point,
        [property: JsonIgnore] Guid UserId
    ) : IRequest;

    public class QuickUpdateTaskItemCommandHandler : IRequestHandler<QuickUpdateTaskItemCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubNotificationService _hubNotificationService;

        public QuickUpdateTaskItemCommandHandler(IUnitOfWork unitOfWork, IHubNotificationService hubNotificationService)
        {
            _unitOfWork = unitOfWork;
            _hubNotificationService = hubNotificationService;
        }

        public async Task Handle(QuickUpdateTaskItemCommand request, CancellationToken cancellationToken)
        {
            var taskItemRepo = _unitOfWork.GetRepository<TaskItem, Guid>();
            var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();
            var userRepo = _unitOfWork.GetRepository<User, Guid>();

            var taskItem = await taskItemRepo.GetByIdAsync(request.Id) ?? throw new AppException(CustomCode.TaskItemNotFound);
            var space = await spaceRepo.GetByIdAsync(taskItem.SpaceId) ?? throw new AppException(CustomCode.SpaceNotFound);

            // Permission Check:
            // 1. User is Space Owner
            // 2. User is the assignee of the task
            bool isOwner = false;
            if (space.SpaceType == SpaceType.Personal)
            {
                isOwner = space.CreatedBy == request.UserId;
            }
            else
            {
                var spaceMemberRepo = _unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();
                var member = await spaceMemberRepo.FirstOrDefaultAsync(sm => sm.SpaceId == space.Id && sm.UserId == request.UserId);
                isOwner = member?.SpaceRole == SpaceRole.Owner;
            }

            bool isAssignee = taskItem.AssignedUserId == request.UserId;

            if (!isOwner && !isAssignee)
            {
                throw new AppException(CustomCode.UnauthorizedAction);
            }

            var oldStatus = taskItem.Status;
            
            if (request.Status.HasValue)
            {
                taskItem.Status = request.Status.Value;
                
                if (taskItem.Status == TaskItemStatus.Completed && oldStatus != TaskItemStatus.Completed)
                {
                    taskItem.CompletedAt = DateTimeOffset.UtcNow;
                }
                else if (taskItem.Status != TaskItemStatus.Completed)
                {
                    taskItem.CompletedAt = null;
                }
            }

            if (request.StartDate.HasValue)
            {
                taskItem.StartDate = request.StartDate.Value;
            }

            if (request.DueDate.HasValue)
            {
                taskItem.DueDate = request.DueDate.Value;
            }

            if (request.Point.HasValue)
            {
                taskItem.Point = request.Point.Value;
            }

            taskItemRepo.Update(taskItem);
            await _unitOfWork.CommitAsync();

            // Notify Space Owner if task is completed by Someone else
            if (request.Status.HasValue && taskItem.Status == TaskItemStatus.Completed && oldStatus != TaskItemStatus.Completed)
            {
                Guid ownerId = Guid.Empty;
                if (space.SpaceType == SpaceType.Personal)
                {
                    ownerId = space.CreatedBy;
                }
                else
                {
                    var spaceMemberRepo = _unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();
                    var ownerMember = await spaceMemberRepo.FirstOrDefaultAsync(sm => sm.SpaceId == space.Id && sm.SpaceRole == SpaceRole.Owner);
                    if (ownerMember != null) ownerId = ownerMember.UserId;
                }

                if (ownerId != Guid.Empty && ownerId != request.UserId)
                {
                    var user = await userRepo.GetByIdAsync(request.UserId);
                    var userName = user?.FullName ?? user?.Email ?? "Member";

                    await _hubNotificationService.SendNotificationToUserAsync(
                        ownerId,
                        "Task Successfully Completed",
                        new
                        {
                            TaskId = taskItem.Id,
                            Title = taskItem.Title,
                            SenderName = userName,
                            SenderAvatarUrl = user?.AvatarUrl,
                            Message = $"The task \"{taskItem.Title}\" has been successfully completed by {userName}."
                        });
                }
            }
        }
    }

    public class QuickUpdateTaskItemCommandValidator : AbstractValidator<QuickUpdateTaskItemCommand>
    {
        public QuickUpdateTaskItemCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            
            When(x => x.Status.HasValue, () => {
                RuleFor(x => x.Status!.Value).IsInEnum();
            });

            RuleFor(x => x)
                .Must(x => !x.StartDate.HasValue || !x.DueDate.HasValue || x.StartDate <= x.DueDate)
                .WithMessage("StartDate must be earlier than DueDate.");

            When(x => x.Point.HasValue, () => {
                RuleFor(x => x.Point!.Value).InclusiveBetween(1, 10).WithMessage("Point must be between 1 and 10.");
            });
        }
    }
}
