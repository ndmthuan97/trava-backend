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
    public record CreateTaskItemCommand(
        Guid SpaceId,
        string Title,
        string? Description,
        TaskItemStatus Status,
        TaskItemPriority Priority,
        int Point,
        DateTimeOffset? StartDate,
        DateTimeOffset? DueDate,
        Guid? AssignedUserId,
        [property: JsonIgnore] Guid CreatedBy
    ) : IRequest<TaskItemResponse>;


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
                if (space.CreatedBy != request.CreatedBy)
                    throw new AppException(CustomCode.UnauthorizedAction);
            }
            else
            {
                var isOwner = await spaceMemberRepo.ExistsAsync(sm =>
                    sm.SpaceId == request.SpaceId &&
                    sm.UserId == request.CreatedBy &&
                    sm.SpaceRole == SpaceRole.Owner);

                if (!isOwner)
                    throw new AppException(CustomCode.UnauthorizedAction);
            }

            var taskItem = _mapper.Map<TaskItem>(request);
            if (request.AssignedUserId.HasValue) taskItem.AssignedAt = DateTimeOffset.UtcNow;

            await taskItemRepo.AddAsync(taskItem);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<TaskItemResponse>(taskItem);
        }
    }

    public class CreateTaskItemCommandValidator : AbstractValidator<CreateTaskItemCommand>
    {
        public CreateTaskItemCommandValidator(IUnitOfWork unitOfWork)
        {
            var spaceRepo = unitOfWork.GetRepository<Space, Guid>();
            var spaceMemberRepo = unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();
            var taskItemRepo = unitOfWork.GetRepository<TaskItem, Guid>();

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(x => x.Description)
                .MaximumLength(4000);

            RuleFor(x => x.Status)
                .IsInEnum();

            RuleFor(x => x.Priority)
                .IsInEnum();

            RuleFor(x => x.Point)
                .InclusiveBetween(1, 10);

            RuleFor(x => x)
                .Must(x => !x.StartDate.HasValue || !x.DueDate.HasValue || x.StartDate <= x.DueDate)
                .WithMessage("StartDate must be earlier than DueDate.");

            RuleFor(x => x.SpaceId)
                .MustAsync((id, ct) => spaceRepo.ExistsAsync(id))
                .WithMessage("Space not found.");

            When(x => x.AssignedUserId.HasValue, () =>
            {
                RuleFor(x => x.AssignedUserId)
                    .MustAsync(async (cmd, assignedUserId, ct) =>
                    {
                        var space = await spaceRepo.GetByIdAsync(cmd.SpaceId);
                        if (space == null) return false;

                        if (space.SpaceType == SpaceType.Personal)
                            return assignedUserId == space.CreatedBy;

                        return await spaceMemberRepo.ExistsAsync(sm =>
                            sm.SpaceId == cmd.SpaceId &&
                            sm.UserId == assignedUserId!.Value);
                    })
                    .WithErrorCode(CustomCode.AssignedUserNotInSpace.ToString())
                    .WithMessage("Assigned user does not belong to this space.");
            });
        }
    }
}