using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Shared.Enums;
using Trava.Application.Interfaces.Services;

namespace Trava.Application.Features.TaskItems.Commands
{
    public record UpdateTaskItemCommand
    (
        [property: JsonIgnore] Guid Id,
        string Title,
        string Description,
        TaskItemStatus Status,
        TaskItemPriority Priority,
        int Point,
        DateTimeOffset? StartDate,
        DateTimeOffset? DueDate,
        Guid? AssignedUserId,
        [property: JsonIgnore] Guid UpdatedBy
    ) : IRequest;

    public class UpdateTaskItemCommandHandler : IRequestHandler<UpdateTaskItemCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubNotificationService _hubNotificationService;

        public UpdateTaskItemCommandHandler(IUnitOfWork unitOfWork, IHubNotificationService hubNotificationService)
        {
            _unitOfWork = unitOfWork;
            _hubNotificationService = hubNotificationService;
        }

        public async Task Handle(UpdateTaskItemCommand request, CancellationToken cancellationToken)
        {
            var taskItemRepo = _unitOfWork.GetRepository<TaskItem, Guid>();
            var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();
            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            
            var taskItem = await taskItemRepo.GetByIdAsync(request.Id) ?? throw new AppException(CustomCode.TaskItemNotFound);
            var space = await spaceRepo.GetByIdAsync(taskItem.SpaceId) ?? throw new AppException(CustomCode.SpaceNotFound);

            var oldAssignedUserId = taskItem.AssignedUserId;
            var oldStatus = taskItem.Status;

            taskItem.Title = request.Title;
            taskItem.Description = request.Description;
            taskItem.Status = request.Status;
            taskItem.Priority = request.Priority;
            taskItem.Point = request.Point;
            taskItem.StartDate = request.StartDate;
            taskItem.DueDate = request.DueDate;
            
            if (taskItem.AssignedUserId != request.AssignedUserId)
            {
                taskItem.AssignedUserId = request.AssignedUserId;
                taskItem.AssignedAt = request.AssignedUserId.HasValue
                    ? DateTimeOffset.UtcNow
                    : null;
            }

            taskItemRepo.Update(taskItem);
            await _unitOfWork.CommitAsync();

            // 1. Notify new assignee if changed
            if (taskItem.AssignedUserId.HasValue && taskItem.AssignedUserId != oldAssignedUserId)
            {
                var updater = await userRepo.GetByIdAsync(request.UpdatedBy);
                await _hubNotificationService.SendNotificationToUserAsync(
                    taskItem.AssignedUserId.Value,
                    "Task Assigned",
                    new
                    {
                        TaskId = taskItem.Id,
                        Title = taskItem.Title,
                        SpaceName = space.Name,
                        SenderName = updater?.FullName ?? updater?.Email ?? "Administrator",
                        SenderAvatarUrl = updater?.AvatarUrl,
                        Message = $"You have been assigned a new task: \"{taskItem.Title}\" in space \"{space.Name}\"."
                    });
            }

            // 2. Notify Space Owner if task is completed
            if (taskItem.Status == TaskItemStatus.Completed && oldStatus != TaskItemStatus.Completed)
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

                if (ownerId != Guid.Empty && ownerId != request.UpdatedBy)
                {
                    var user = await userRepo.GetByIdAsync(request.UpdatedBy);
                    var userName = user?.FullName ?? user?.Email ?? "Member";

                    await _hubNotificationService.SendNotificationToUserAsync(
                        ownerId,
                        "Task Completed",
                        new
                        {
                            TaskId = taskItem.Id,
                            Title = taskItem.Title,
                            SenderName = userName,
                            SenderAvatarUrl = user?.AvatarUrl,
                            Message = $"The task \"{taskItem.Title}\" was completed by {userName}."
                        });
                }
            }
        }
    }

    public class UpdateTaskItemCommandValidator : AbstractValidator<UpdateTaskItemCommand>
    {
        public UpdateTaskItemCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.UpdatedBy).NotEmpty();

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(300).WithMessage("Title must not exceed 300 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value.");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Invalid priority value.");

            RuleFor(x => x.Point)
                .InclusiveBetween(1, 10).WithMessage("Point must be between 1 and 10.");

            RuleFor(x => x)
                .Must(x => !x.StartDate.HasValue || !x.DueDate.HasValue || x.StartDate <= x.DueDate)
                .WithMessage("StartDate must be earlier than DueDate.");
        }
    }
}