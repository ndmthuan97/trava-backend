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
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.Application.Features.TaskItems.Commands
{
    public record AssigneTaskItemCommand([property: JsonIgnore] Guid Id, Guid AssignedUserId, [property: JsonIgnore] Guid CreatedBy) : IRequest;

    public class AssigneTaskItemCommandHandler : IRequestHandler<AssigneTaskItemCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AssigneTaskItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(AssigneTaskItemCommand request, CancellationToken cancellationToken)
        {
            var taskItemRepo = _unitOfWork.GetRepository<TaskItem, Guid>();
            var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();
            var spaceMemberRepo = _unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();

            var taskItem = await taskItemRepo.GetByIdAsync(request.Id) ?? throw new AppException(CustomCode.TaskItemNotFound);
            var space = await spaceRepo.GetByIdAsync(taskItem.SpaceId) ?? throw new AppException(CustomCode.SpaceNotFound);

            if (space.SpaceType == SpaceType.Personal)
            {
                if (space.CreatedBy != request.CreatedBy)
                    throw new AppException(CustomCode.UnauthorizedAction);

                if (request.AssignedUserId != space.CreatedBy)
                    throw new AppException(CustomCode.AssignedUserNotInSpace);
            }
            else
            {
                var member = await spaceMemberRepo.FirstOrDefaultAsync(sm =>
                    sm.SpaceId == taskItem.SpaceId &&
                    sm.UserId == request.CreatedBy
                );

                if (member == null || member.Role != SpaceRole.Owner)
                    throw new AppException(CustomCode.UnauthorizedAction);

                var isAssignedMember = await spaceMemberRepo.ExistsAsync(sm =>
                    sm.SpaceId == taskItem.SpaceId &&
                    sm.UserId == request.AssignedUserId
                );

                if (!isAssignedMember)
                    throw new AppException(CustomCode.AssignedUserNotInSpace);
            }

            taskItem.AssignedUserId = request.AssignedUserId;
            taskItem.AssignedAt = DateTimeOffset.UtcNow;

            taskItemRepo.Update(taskItem);
            await _unitOfWork.CommitAsync();
        }
    }

    public class AssigneTaskItemCommandValidator : AbstractValidator<AssigneTaskItemCommand>
    {
        public AssigneTaskItemCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Invalid task.");

            RuleFor(x => x.AssignedUserId)
                .NotEmpty()
                .WithMessage("Please select a user to assign this task to.");

            RuleFor(x => x.CreatedBy)
                .NotEmpty()
                .WithMessage("Unable to identify the user performing this action.");
        }
    }
}