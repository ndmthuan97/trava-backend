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

namespace Trava.Application.Features.TaskItems.Commands
{
    public class UpdateTaskItemCommand : IRequest<Unit>
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public TaskItemStatus Status { get; set; } = TaskItemStatus.NotStart;
        public TaskItemPriority Priority { get; set; } = TaskItemPriority.Low;
        public int Point { get; set; } = 1;
        public DateTimeOffset? StartDate { get; set; } = null;
        public DateTimeOffset? DueDate { get; set; } = null;
        public Guid? AssignedUserId { get; set; }
    }

    public class UpdateTaskItemCommandHandler : IRequestHandler<UpdateTaskItemCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTaskItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateTaskItemCommand request, CancellationToken cancellationToken)
        {
            var taskItemRepo = _unitOfWork.GetRepository<TaskItem, Guid>();
            var taskItem = await taskItemRepo.GetByIdAsync(request.Id) ?? throw new AppException(CustomCode.TaskItemNotFound);

            taskItem.Title = request.Title;
            taskItem.Description = request.Description;
            taskItem.Status = request.Status;
            taskItem.Priority = request.Priority;
            taskItem.Point = request.Point;
            taskItem.StartDate = request.StartDate;
            taskItem.DueDate = request.DueDate;
            taskItem.AssignedUserId = request.AssignedUserId;

            if (request.AssignedUserId.HasValue)
            {
                taskItem.AssignedAt = DateTimeOffset.UtcNow;
            }

            taskItemRepo.Update(taskItem);
            await _unitOfWork.CommitAsync();

            return Unit.Value;
        }
    }

    public class UpdateTaskItemCommandValidator : AbstractValidator<UpdateTaskItemCommand>
    {
        public UpdateTaskItemCommandValidator()
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