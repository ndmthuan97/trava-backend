using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Domain.Enums;
using Trava.Shared.Enums;

namespace Trava.Application.Features.Users.Commands
{
    public record UpdateUserStatusCommand([property: JsonIgnore] Guid Id, UserStatus Status) : IRequest;

    public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
        {
            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            var user = await userRepo.GetByIdAsync(request.Id);

            if (user == null)
            {
                throw new AppException(CustomCode.UserNotExists);
            }

            user.Status = request.Status;
            user.LastModifiedAt = DateTimeOffset.UtcNow;

            userRepo.Update(user);
            await _unitOfWork.CommitAsync();
        }
    }

    public class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
    {
        public UpdateUserStatusCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Status).IsInEnum();
        }
    }
}
