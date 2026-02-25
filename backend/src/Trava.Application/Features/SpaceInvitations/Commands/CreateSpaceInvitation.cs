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
using Trava.Application.Interfaces.Services;

namespace Trava.Application.Features.SpaceInvitations.Commands
{
    public record CreateSpaceInvitationCommand(
        Guid SpaceId,
        Guid InvitedUserId
    ) : IRequest<SpaceInvitationResponse>;

    public class CreateSpaceInvitationCommandHandler : IRequestHandler<CreateSpaceInvitationCommand, SpaceInvitationResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubNotificationService _hubNotificationService;

        public CreateSpaceInvitationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHubNotificationService hubNotificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubNotificationService = hubNotificationService;
        }

        public async Task<SpaceInvitationResponse> Handle(CreateSpaceInvitationCommand request, CancellationToken cancellationToken)
        {
            var spaceInvitationRepo = _unitOfWork.GetRepository<SpaceInvitation, Guid>();
            var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();
            var userRepo = _unitOfWork.GetRepository<User, Guid>();

            var spaceInvitaion = _mapper.Map<SpaceInvitation>(request);
            await spaceInvitationRepo.AddAsync(spaceInvitaion);
            await _unitOfWork.CommitAsync();

            // Send Notification
            var space = await spaceRepo.GetByIdAsync(request.SpaceId);
            if (space != null)
            {
                var owner = await userRepo.GetByIdAsync(space.CreatedBy);
                var inviterName = owner?.FullName ?? owner?.Email ?? "Administrator";
                var inviterAvatar = owner?.AvatarUrl;

                await _hubNotificationService.SendNotificationToUserAsync(
                    request.InvitedUserId,
                    "New Workspace Invitation",
                    new
                    {
                        SpaceId = space.Id,
                        SpaceName = space.Name,
                        SenderName = inviterName,
                        SenderAvatarUrl = inviterAvatar,
                        Message = $"{inviterName} has invited you to join the workspace \"{space.Name}\". Join now to start collaborating with the team!"
                    });
            }

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
        }
    }
}