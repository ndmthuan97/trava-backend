using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Features.Users.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Shared.Enums;

namespace Trava.Application.Features.Users.Queries
{
    public record GetUserProfileQuery(Guid UserId) : IRequest<UserResponse>;

    public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, UserResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserProfileHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            var user = await userRepo.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new AppException(CustomCode.UserNotExists);
            }

            return _mapper.Map<UserResponse>(user);
        }
    }
}
