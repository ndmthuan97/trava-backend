using MediatR;
using Microsoft.Extensions.Logging;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Services;
using Trava.Domain.Entities;
using Trava.Shared.Enums;
using FluentValidation;

namespace Trava.Application.Features.Auth.Commands;

public record RegisterCommand(string FullName, string Email, string Password) : IRequest<CustomCode>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, CustomCode>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(IUnitOfWork unitOfWork, ILogger<RegisterCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CustomCode> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userRepo = _unitOfWork.GetRepository<User, Guid>();
        var existingUser = await userRepo.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Attempt to register with existing email: {Email}.", request.Email);
            throw new AppException(CustomCode.EmailAlreadyExists);
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        await userRepo.AddAsync(newUser);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("New user registered with email: {Email}.", request.Email);
        return CustomCode.Success;
    }
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
    }
}
