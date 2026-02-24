using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Features.TaskItems.Responses;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Services;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.Application.Features.TaskItems.Commands
{
    public record CreateTaskCommentCommand(
        Guid TaskItemId,
        string Content,
        [property: JsonIgnore] Guid UserId
    ) : IRequest<TaskCommentResponse>;

    public class CreateTaskCommentCommandHandler : IRequestHandler<CreateTaskCommentCommand, TaskCommentResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubNotificationService _hubNotificationService;

        public CreateTaskCommentCommandHandler(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IHubNotificationService hubNotificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubNotificationService = hubNotificationService;
        }

        public async Task<TaskCommentResponse> Handle(CreateTaskCommentCommand request, CancellationToken cancellationToken)
        {
            var taskItemRepo = _unitOfWork.GetRepository<TaskItem, Guid>();
            var commentRepo = _unitOfWork.GetRepository<TaskComment, Guid>();
            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();

            var taskItem = await taskItemRepo.FirstOrDefaultAsync(x => x.Id == request.TaskItemId, 
                include: q => q) ?? throw new AppException(CustomCode.TaskItemNotFound);

            var user = await userRepo.GetByIdAsync(request.UserId) ?? throw new AppException(CustomCode.UserNotExists);

            var comment = new TaskComment
            {
                TaskItemId = request.TaskItemId,
                UserId = request.UserId,
                Content = request.Content
            };

            await commentRepo.AddAsync(comment);
            await _unitOfWork.CommitAsync();

            // Prepare notification
            var commenterName = user.FullName ?? user.Email ?? "User";
            var notificationPayload = new
            {
                TaskId = taskItem.Id,
                TaskTitle = taskItem.Title,
                CommentId = comment.Id,
                CommentContent = comment.Content,
                SenderName = commenterName,
                SenderAvatarUrl = user.AvatarUrl,
                Message = $"{commenterName} commented on task \"{taskItem.Title}\"."
            };

            // Notify relevant people: Assignee and Space Owner
            // 1. Notify Assignee if not the commenter
            if (taskItem.AssignedUserId.HasValue && taskItem.AssignedUserId.Value != request.UserId)
            {
                await _hubNotificationService.SendNotificationToUserAsync(
                    taskItem.AssignedUserId.Value,
                    "New Comment",
                    notificationPayload);
            }

            // 2. Notify Space Owner if not the commenter and not the assignee (already notified)
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
                    var ownerMember = await spaceMemberRepo.FirstOrDefaultAsync(sm => sm.SpaceId == space.Id && sm.SpaceRole == SpaceRole.Owner);
                    if (ownerMember != null) ownerId = ownerMember.UserId;
                }

                if (ownerId != Guid.Empty && ownerId != request.UserId && ownerId != taskItem.AssignedUserId)
                {
                    await _hubNotificationService.SendNotificationToUserAsync(
                        ownerId,
                        "New Comment",
                        notificationPayload);
                }
            }

            var response = _mapper.Map<TaskCommentResponse>(comment);
            return response with { UserFullName = user.FullName, UserAvatarUrl = user.AvatarUrl };
        }
    }

    public class CreateTaskCommentCommandValidator : AbstractValidator<CreateTaskCommentCommand>
    {
        public CreateTaskCommentCommandValidator()
        {
            RuleFor(x => x.TaskItemId).NotEmpty();
            RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
