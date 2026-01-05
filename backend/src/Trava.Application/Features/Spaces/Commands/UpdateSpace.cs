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
    public record UpdateSpaceCommand([property: JsonIgnore] Guid Id, string Name, string Description) : IRequest<Unit>;

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
            await _unitOfWork.CommitAsync();
            return Unit.Value;
        }
    }

    public class UpdateSpaceCommandValidator : AbstractValidator<UpdateSpaceCommand>
    {
        public UpdateSpaceCommandValidator(IUnitOfWork unitOfWork)
        {
            var spaceRepo = unitOfWork.GetRepository<Space, Guid>();

            RuleFor(x => x.Id)
                .NotEmpty()
                .MustAsync(async (id, ct) =>
                    await spaceRepo.ExistsAsync(id))
                .WithMessage("Space not found.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(1000);

            RuleFor(x => x)
                .MustAsync(async (command, ct) =>
                {
                    var space = await spaceRepo.GetByIdAsync(command.Id);
                    if (space == null) return false;

                    // Nếu không đổi tên → OK
                    if (space.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase))
                        return true;

                    return !await spaceRepo.ExistsAsync(s =>
                        s.CreatedBy == space.CreatedBy &&
                        s.Name.ToLower() == command.Name.ToLower() &&
                        s.Id != command.Id);
                })
                .WithErrorCode(CustomCode.SpaceNameAlreadyExists.ToString())
                .WithMessage("Space name already exists.");
        }
    }

}