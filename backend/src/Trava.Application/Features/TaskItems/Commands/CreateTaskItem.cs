using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Features.TaskItems.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.Application.Features.TaskItems.Commands
{
    public class CreateTaskItemCommand : IRequest<TaskItemResponse>
    {
        public Guid SpaceId { get; set; }
        public Guid? ParentTaskId { get; set; } = null;
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public TaskItemStatus Status { get; set; } = TaskItemStatus.NotStart;
        public TaskItemPriority Priority { get; set; } = TaskItemPriority.Low;
        public int Point { get; set; } = 1;
        public DateTimeOffset? StartDate { get; set; } = null;
        public DateTimeOffset? DueDate { get; set; } = null;
        public Guid? AssignedUserId { get; set; }
        [JsonIgnore]
        public Guid CreatedBy { get; set; }
    }

    public class CreateTaskItemCommandHandler : IRequestHandler<CreateTaskItemCommand, TaskItemResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CreateTaskItemCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<TaskItemResponse> Handle(CreateTaskItemCommand request, CancellationToken cancellationToken)
        {
            var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();
            var spaceMemberRepo = _unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();
            var taskItemRepo = _unitOfWork.GetRepository<TaskItem, Guid>();

            var space = await spaceRepo.GetByIdAsync(request.SpaceId) ?? throw new AppException(CustomCode.SpaceNotFound);
            if (space.SpaceType == SpaceType.Personal)
            {
                if (space.CreatedBy != request.CreatedBy) throw new AppException(CustomCode.UnauthorizedAction);
            }
            else
            {
                var isMember = await spaceMemberRepo.ExistsAsync(sm => sm.SpaceId == request.SpaceId && sm.UserId == request.CreatedBy && sm.Role == SpaceRole.Owner);
                if (!isMember) throw new AppException(CustomCode.UnauthorizedAction);
            }

            if (request.AssignedUserId.HasValue)
            {
                if (space.SpaceType == SpaceType.Personal && request.AssignedUserId != space.CreatedBy)
                    throw new AppException(CustomCode.AssignedUserNotInSpace);
                else
                {
                    var isAssignedUserMember = await spaceMemberRepo.ExistsAsync(sm => sm.SpaceId == request.SpaceId && sm.UserId == request.AssignedUserId.Value);
                    if (!isAssignedUserMember) throw new AppException(CustomCode.AssignedUserNotInSpace);
                }
            }

            if (request.ParentTaskId.HasValue)
            {
                var parentTask = await taskItemRepo.GetByIdAsync(request.ParentTaskId.Value) ?? throw new AppException(CustomCode.ParentTaskItemNotFound);
                if (parentTask.SpaceId != request.SpaceId) throw new AppException(CustomCode.ParentTaskItemNotExistInSpace);
            }

            var taskItem = _mapper.Map<TaskItem>(request);
            if (request.AssignedUserId.HasValue)
            {
                taskItem.AssignedAt = DateTimeOffset.UtcNow;
            }

            await taskItemRepo.AddAsync(taskItem);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<TaskItemResponse>(taskItem);
        }
    }

    public class CreateTaskItemCommandValidator : AbstractValidator<CreateTaskItemCommand>
    {
        public CreateTaskItemCommandValidator()
        {
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