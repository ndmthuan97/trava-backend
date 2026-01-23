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

namespace Trava.Application.Features.TaskItems.Commands
{
    public record CompleteTaskItemCommand([property: JsonIgnore] Guid Id, [property: JsonIgnore] Guid CompletedBy) : IRequest;

    public class CompleteTaskItemCommandHandler : IRequestHandler<CompleteTaskItemCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompleteTaskItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CompleteTaskItemCommand request, CancellationToken cancellationToken)
        {
            var taskItemRepo = _unitOfWork.GetRepository<TaskItem, Guid>();
            var taskItem = await taskItemRepo.GetByIdAsync(request.Id) ?? throw new AppException(CustomCode.TaskItemNotFound);

            if (taskItem.AssignedUserId != request.CompletedBy)
            {
                throw new AppException(CustomCode.UnauthorizedAction);
            }

            taskItem.CompletedAt = DateTimeOffset.UtcNow;
            taskItemRepo.Update(taskItem);
            await _unitOfWork.CommitAsync();
        }
    }

    public class CompleteTaskItemCommandValidator : AbstractValidator<CompleteTaskItemCommand>
    {
        public CompleteTaskItemCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Task item ID is required.");
            RuleFor(x => x.CompletedBy).NotEmpty().WithMessage("CompletedBy user ID is required.");
        }
    }
}