using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trava.Application.Common.Exceptions;
using Trava.Application.Features.Spaces.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.Application.Features.Spaces.Commands
{
    public record CreateSpaceCommand(
        string Name,
        string? Description,
        SpaceType SpaceType,
        [property: JsonIgnore] Guid CreatedBy
    ) : IRequest<SpaceResponse>;

    public class CreateSpaceCommandHandler : IRequestHandler<CreateSpaceCommand, SpaceResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateSpaceCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SpaceResponse> Handle(CreateSpaceCommand request, CancellationToken cancellationToken)
        {
            var _spaceRepository = _unitOfWork.GetRepository<Space, Guid>();
            var _spaceMemberRepository = _unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();

            var space = _mapper.Map<Space>(request);
            await _spaceRepository.AddAsync(space);

            if (request.SpaceType == SpaceType.Team)
            {
                var spaceMember = new SpaceMember
                {
                    SpaceId = space.Id,
                    UserId = request.CreatedBy,
                    SpaceRole = SpaceRole.Owner,
                };
                await _spaceMemberRepository.AddAsync(spaceMember);
            }
            await _unitOfWork.CommitAsync();
            return _mapper.Map<SpaceResponse>(space);
        }
    }

    public class CreateSpaceCommandValidator : AbstractValidator<CreateSpaceCommand>
    {
        public CreateSpaceCommandValidator(IUnitOfWork unitOfWork)
        {
            var spaceRepo = unitOfWork.GetRepository<Space, Guid>();

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Space name is required.")
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(1000);

            RuleFor(x => x.SpaceType)
                .IsInEnum();

            RuleFor(x => x.CreatedBy)
                .NotEmpty();

            RuleFor(x => x)
                .MustAsync(async (command, ct) =>
                    !await spaceRepo.ExistsAsync(s =>
                        s.CreatedBy == command.CreatedBy &&
                        s.Name.ToLower() == command.Name.ToLower()))
                .WithErrorCode(CustomCode.SpaceNameAlreadyExists.ToString())
                .WithMessage("Space name already exists.");
        }
    }

}