using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Trava.Application.Features.SpaceInvitations.Responses;
using Trava.Application.Features.Spaces.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Domain.Enums;

namespace Trava.Application.Features.SpaceInvitations.Commands
{
    public record CreateSpaceInvitationCommand(
        Guid SpaceId,
        Guid InvitedUserId,
        DateTime? ExpiredAt
    ) : IRequest<SpaceInvitationResponse>;

    public class CreateSpaceInvitationCommandHandler : IRequestHandler<CreateSpaceInvitationCommand, SpaceInvitationResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateSpaceInvitationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SpaceInvitationResponse> Handle(CreateSpaceInvitationCommand request, CancellationToken cancellationToken)
        {
            var spaceInvitationRepo = _unitOfWork.GetRepository<SpaceInvitation, Guid>();

            var spaceInvitaion = _mapper.Map<SpaceInvitation>(request);
            await spaceInvitationRepo.AddAsync(spaceInvitaion);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<SpaceInvitationResponse>(spaceInvitaion);
        }
    }

    public class CreateSpaceInvitationCommandValidator
        : AbstractValidator<CreateSpaceInvitationCommand>
    {
        public CreateSpaceInvitationCommandValidator(IUnitOfWork unitOfWork)
        {
            var userRepo = unitOfWork.GetRepository<User, Guid>();
            var spaceRepo = unitOfWork.GetRepository<Space, Guid>();
            var invitationRepo = unitOfWork.GetRepository<SpaceInvitation, Guid>();

            RuleFor(x => x.SpaceId)
                .NotEmpty()
                .MustAsync(async (id, ct) =>
                    await spaceRepo.ExistsAsync(s => s.Id == id))
                .WithMessage("Space does not exist.");

            RuleFor(x => x.InvitedUserId)
                .NotEmpty()
                .MustAsync(async (id, ct) =>
                    await userRepo.ExistsAsync(u => u.Id == id))
                .WithMessage("User does not exist.");
                
            When(x => x.ExpiredAt.HasValue, () =>
            {
                RuleFor(x => x.ExpiredAt!.Value)
                    .Must(x => x > DateTime.UtcNow)
                    .WithMessage("ExpiredAt must be in the future.");
            });
        }
    }
}