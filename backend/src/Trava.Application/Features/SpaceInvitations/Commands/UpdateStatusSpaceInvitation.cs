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
using Trava.Application.Interfaces.Services;

namespace Trava.Application.Features.SpaceInvitations.Commands
{
    public record UpdateStatusSpaceInvitationCommand
    (
        [property: JsonIgnore] Guid Id,
        InvitationStatus InvitationStatus,
        [property: JsonIgnore] Guid InvitatedUser
    ) : IRequest;


    public class UpdateStatusSpaceInvitationCommandHandler
    : IRequestHandler<UpdateStatusSpaceInvitationCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubNotificationService _hubNotificationService;

        public UpdateStatusSpaceInvitationCommandHandler(IUnitOfWork unitOfWork, IHubNotificationService hubNotificationService)
        {
            _unitOfWork = unitOfWork;
            _hubNotificationService = hubNotificationService;
        }

        public async Task Handle(UpdateStatusSpaceInvitationCommand request, CancellationToken cancellationToken)
        {
            var invitationRepo = _unitOfWork.GetRepository<SpaceInvitation, Guid>();
            var spaceMemberRepo = _unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();
            var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();
            var userRepo = _unitOfWork.GetRepository<User, Guid>();

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
                    SpaceRole = SpaceRole.Member,
                };
                await spaceMemberRepo.AddAsync(spaceMember);
            }

            await _unitOfWork.CommitAsync();

            // Send Notification
            var space = await spaceRepo.GetByIdAsync(invitation.SpaceId);
            if (space != null)
            {
                var invitedUser = await userRepo.GetByIdAsync(invitation.InvitedUserId);
                var invitedUserName = invitedUser?.FullName ?? invitedUser?.Email ?? "User";
                
                if (invitation.Status == InvitationStatus.Accepted)
                {
                    // Notify Owner
                    await _hubNotificationService.SendNotificationToUserAsync(
                        space.CreatedBy,
                        "SpaceInvitationAccepted",
                        new
                        {
                            SpaceId = space.Id,
                            SpaceName = space.Name,
                            UserName = invitedUserName,
                            Message = $"{invitedUserName} accepted the invitation to join space \"{space.Name}\"."
                        });
 
                    // Welcome Notification to User who joined
                    await _hubNotificationService.SendNotificationToUserAsync(
                        invitation.InvitedUserId,
                        "SpaceWelcome",
                        new
                        {
                            SpaceId = space.Id,
                            SpaceName = space.Name,
                            Message = $"Welcome to {space.Name}! You can now start working, connecting with members and collaborating effectively."
                        });
                }
                else
                {
                    // Notify Owner of rejection
                    await _hubNotificationService.SendNotificationToUserAsync(
                        space.CreatedBy,
                        "SpaceInvitationRejected",
                        new
                        {
                            SpaceId = space.Id,
                            SpaceName = space.Name,
                            UserName = invitedUserName,
                            Message = $"{invitedUserName} rejected the invitation to join space \"{space.Name}\"."
                        });
                }
            }
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