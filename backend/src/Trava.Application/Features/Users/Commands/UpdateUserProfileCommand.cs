using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Shared.Enums;

namespace Trava.Application.Features.Users.Commands
{
    public record UpdateUserProfileCommand(
        [property: JsonIgnore] Guid Id,
        string FullName,
        string? PhoneNumber,
        DateTime? BirthDate,
        string? AvatarUrl
    ) : IRequest;

    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            var user = await userRepo.GetByIdAsync(request.Id);

            if (user == null)
            {
                throw new AppException(CustomCode.UserNotExists);
            }

            user.FullName = request.FullName;
            user.Phone = request.PhoneNumber;
            user.BirthDate = request.BirthDate;
            if (!string.IsNullOrEmpty(request.AvatarUrl))
            {
                user.AvatarUrl = request.AvatarUrl;
            }
            
            user.LastModifiedAt = DateTimeOffset.UtcNow;

            userRepo.Update(user);
            await _unitOfWork.CommitAsync();
        }
    }

    public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full Name is required.")
                .MaximumLength(200).WithMessage("Full Name must not exceed 200 characters.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^((03|05|07|08|09)\d{8}|02\d{9})$").WithMessage("Invalid phone number format")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.AvatarUrl)
                .MaximumLength(255).WithMessage("Avatar URL must not exceed 255 characters.")
                .Must(url => string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage("Avatar URL must be a valid absolute URL.")
                .When(x => !string.IsNullOrEmpty(x.AvatarUrl));
        }
    }
}
