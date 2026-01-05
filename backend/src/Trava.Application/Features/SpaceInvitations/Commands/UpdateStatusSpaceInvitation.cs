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

namespace Trava.Application.Features.SpaceInvitations.Commands
{
    public record UpdateStatusSpaceInvitationCommand
    (
        [property: JsonIgnore] Guid Id,
        InvitationStatus InvitationStatus,
        [property: JsonIgnore] Guid InvitatedUser
    ) : IRequest<Unit>;


    public class UpdateStatusSpaceInvitationCommandHandler
    : IRequestHandler<UpdateStatusSpaceInvitationCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateStatusSpaceInvitationCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateStatusSpaceInvitationCommand request, CancellationToken cancellationToken)
        {
            var invitationRepo = _unitOfWork.GetRepository<SpaceInvitation, Guid>();
            var spaceMemberRepo = _unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();

            var invitation = await invitationRepo.GetByIdAsync(request.Id)
                ?? throw new AppException(CustomCode.SpaceInvitationNotFound);

            if (invitation.InvitedUserId != request.InvitatedUser)
                throw new AppException(CustomCode.UnauthorizedAction);

            if (invitation.Status != InvitationStatus.Pending)
                throw new AppException(CustomCode.InvalidInvitationStatusTransition);

            if (request.InvitationStatus is not (InvitationStatus.Accepted or InvitationStatus.Rejected))
                throw new AppException(CustomCode.InvalidInvitationStatusTransition);

            invitation.Status = request.InvitationStatus;

            invitationRepo.Update(invitation);
            
            if (invitation.Status == InvitationStatus.Accepted)
            {
                var spaceMember = new SpaceMember
                {
                    SpaceId = invitation.SpaceId,
                    UserId = invitation.InvitedUserId,
                    Role = SpaceRole.Member,
                };
                await spaceMemberRepo.AddAsync(spaceMember);
            }

            await _unitOfWork.CommitAsync();

            return Unit.Value;
        }
    }
    public class UpdateStatusSpaceInvitationCommandValidator : AbstractValidator<UpdateStatusSpaceInvitationCommand>
    {
        public UpdateStatusSpaceInvitationCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Invalid invitation.");

            RuleFor(x => x.InvitatedUser)
                .NotEmpty()
                .WithMessage("Unable to identify the invited user.");

            RuleFor(x => x.InvitationStatus)
                .IsInEnum()
                .WithMessage("Invalid invitation status.");

            RuleFor(x => x.InvitationStatus)
                .Must(status =>
                    status == InvitationStatus.Accepted ||
                    status == InvitationStatus.Rejected)
                .WithMessage("Invitation can only be accepted or rejected.");
        }
    }
}