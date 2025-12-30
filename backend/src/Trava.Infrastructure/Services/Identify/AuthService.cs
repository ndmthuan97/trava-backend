using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Trava.Application.Common.Exceptions;
using Trava.Application.Features.Auth.DTOs;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Services;
using Trava.Domain.Entities;
using Trava.Shared.Enums;

namespace Trava.Infrastructure.Services.Identify
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtHandler _jwtHandler;
        private readonly IServiceProvider _serviceProvider;

        public AuthService(
            IUnitOfWork unitOfWork,
            ILogger<AuthService> logger,
            JwtHandler jwtHandler,
            IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _jwtHandler = jwtHandler;
            _serviceProvider = serviceProvider;
        }

        public Task<(CustomCode, AuthResultDto)> LoginAsync(LoginRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomCode> RegisterAsync(RegisterRequestDto request)
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
                EmailConfirmed = true,
            };

            await userRepo.AddAsync(newUser);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("New user registered with email: {Email}.", request.Email);
            return CustomCode.Success;
        }
    }
}