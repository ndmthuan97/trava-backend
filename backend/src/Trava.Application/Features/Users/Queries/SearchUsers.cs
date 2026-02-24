using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trava.Application.Features.Users.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Domain.Enums;

namespace Trava.Application.Features.Users.Queries
{
    public record SearchUsersQuery(string? SearchTerm) : IRequest<List<UserSearchResponse>>;

    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, List<UserSearchResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SearchUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<UserSearchResponse>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var userRepo = _unitOfWork.GetRepository<User, Guid>();
            
            var users = await userRepo.GetListAsync(u => 
                u.Status == UserStatus.Active &&
                (string.IsNullOrWhiteSpace(request.SearchTerm) ||
                 EF.Functions.ILike(u.FullName, $"%{request.SearchTerm}%") ||
                 EF.Functions.ILike(u.Email, $"%{request.SearchTerm}%")),
                cancellationToken: cancellationToken);

            return _mapper.Map<List<UserSearchResponse>>(users);
        }
    }
}
