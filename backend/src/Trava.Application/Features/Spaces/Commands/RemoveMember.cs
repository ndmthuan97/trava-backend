using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.Application.Features.Spaces.Commands
{
    public record RemoveMemberCommand(
        Guid SpaceId,
        Guid UserId,
        [property: JsonIgnore] Guid RequestedBy
    ) : IRequest;

    public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveMemberCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
        {
            var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();
            var spaceMemberRepo = _unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();

            var space = await spaceRepo.GetByIdAsync(request.SpaceId);
            if (space == null)
            {
                throw new AppException(CustomCode.SpaceNotFound);
            }

            // Authorization: Only owner can remove members
            bool isAuthorized = false;
            if (space.CreatedBy == request.RequestedBy)
            {
                isAuthorized = true;
            }
            else if (space.SpaceType == SpaceType.Team)
            {
                var requester = await spaceMemberRepo.FirstOrDefaultAsync(m => m.SpaceId == space.Id && m.UserId == request.RequestedBy);
                if (requester != null && requester.SpaceRole == SpaceRole.Owner)
                {
                    isAuthorized = true;
                }
            }

            if (!isAuthorized)
            {
                throw new AppException(CustomCode.UnauthorizedAction);
            }

            var memberToRemove = await spaceMemberRepo.FirstOrDefaultAsync(m => m.SpaceId == request.SpaceId && m.UserId == request.UserId);
            if (memberToRemove == null)
            {
                throw new AppException(CustomCode.MemberNotFoundInSpace);
            }

            // Cannot remove the owner creator
            if (memberToRemove.UserId == space.CreatedBy)
            {
                throw new AppException(CustomCode.UnauthorizedAction);
            }

            spaceMemberRepo.Remove(memberToRemove);
            await _unitOfWork.CommitAsync();
        }
    }

    public class RemoveMemberCommandValidator : AbstractValidator<RemoveMemberCommand>
    {
        public RemoveMemberCommandValidator()
        {
            RuleFor(x => x.SpaceId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.RequestedBy).NotEmpty();
        }
    }
}
