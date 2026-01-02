using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Shared.Enums;

namespace Trava.Application.Features.Spaces.Commands
{
    public class UpdateSpaceCommand : IRequest<Unit>
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
    }

    public class UpdateSpaceCommandHandler : IRequestHandler<UpdateSpaceCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateSpaceCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateSpaceCommand request, CancellationToken cancellationToken)
        {
            var _spaceRepository = _unitOfWork.GetRepository<Space, Guid>();

            var space = await _spaceRepository.GetByIdAsync(request.Id) ?? throw new AppException(CustomCode.SpaceNotFound);

            space.Name = request.Name;
            space.Description = request.Description;

            _spaceRepository.Update(space);

            try
            {
                await _unitOfWork.CommitAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Spaces_CreatedBy_Name"))
            {
                throw new AppException(CustomCode.SpaceNameAlreadyExists);
            }

            return Unit.Value;
        }
    }

    public class UpdateSpaceCommandValidator : AbstractValidator<UpdateSpaceCommand>
    {
        public UpdateSpaceCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Space Name is required")
                .MaximumLength(200).WithMessage("Space Name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
        }
    }
}