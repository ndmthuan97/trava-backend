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
    public record DeleteSpaceCommand(
        [property: JsonIgnore] Guid Id,
        [property: JsonIgnore] Guid DeletedBy
    ) : IRequest;

    public class DeleteSpaceCommandHandler : IRequestHandler<DeleteSpaceCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteSpaceCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteSpaceCommand request, CancellationToken cancellationToken)
        {
            var spaceRepository = _unitOfWork.GetRepository<Space, Guid>();
            var space = await spaceRepository.GetByIdAsync(request.Id);

            if (space == null)
            {
                throw new AppException(CustomCode.SpaceNotFound, "Space not found.");
            }

            // Authorization: Only owner can delete
            if (space.CreatedBy != request.DeletedBy)
            {
                // If it's a team space, check if user is an owner in SpaceMembers
                if (space.SpaceType == SpaceType.Team)
                {
                    var spaceMemberRepository = _unitOfWork.GetRepository<SpaceMember, (Guid SpaceId, Guid UserId)>();
                    var member = await spaceMemberRepository.FirstOrDefaultAsync(m => m.SpaceId == space.Id && m.UserId == request.DeletedBy);

                    if (member == null || member.SpaceRole != SpaceRole.Owner)
                    {
                        throw new AppException(CustomCode.UnauthorizedAction, "You do not have permission to delete this space.");
                    }
                }
                else
                {
                    throw new AppException(CustomCode.UnauthorizedAction, "You do not have permission to delete this space.");
                }
            }

            space.DeletedBy = request.DeletedBy;
            spaceRepository.Remove(space);
            await _unitOfWork.CommitAsync();
        }
    }

    public class DeleteSpaceCommandValidator : AbstractValidator<DeleteSpaceCommand>
    {
        public DeleteSpaceCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Space ID is required.");
            RuleFor(x => x.DeletedBy).NotEmpty().WithMessage("User ID is required.");
        }
    }
}
