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
    public class CreateSpaceCommand : IRequest<SpaceResponse>
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public SpaceType SpaceType { get; set; } = SpaceType.Personal;
        [JsonIgnore]
        public Guid CreatedBy { get; set; }
    }

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
                    Role = SpaceRole.Owner,
                };
                await _spaceMemberRepository.AddAsync(spaceMember);
            }

            try
            {
                await _unitOfWork.CommitAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Spaces_CreatedBy_Name"))
            {
                throw new AppException(CustomCode.SpaceNameAlreadyExists);
            }


            return _mapper.Map<SpaceResponse>(space);
        }
    }

    public class CreateSpaceCommandValidator : AbstractValidator<CreateSpaceCommand>
    {
        public CreateSpaceCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Space name is required.")
                .MaximumLength(200).WithMessage("Space name must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(x => x.SpaceType)
                .IsInEnum().WithMessage("Invalid SpaceType.");

            RuleFor(x => x.CreatedBy)
                .NotEmpty().WithMessage("CreatedBy is required.");
        }
    }
}