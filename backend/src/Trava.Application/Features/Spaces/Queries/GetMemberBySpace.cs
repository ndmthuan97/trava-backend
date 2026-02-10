using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Trava.Application.Common.Models;
using Trava.Application.Features.Spaces.Specifications;
using Trava.Application.Features.Users.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Spaces.Queries
{
    public record GetSpaceMembersQuery(SpaceMemberSpecParam Param) : IRequest<Pagination<UserResponse>>;

    public class GetSpaceMembersQueryHandler : IRequestHandler<GetSpaceMembersQuery, Pagination<UserResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetSpaceMembersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Pagination<UserResponse>> Handle(GetSpaceMembersQuery request, CancellationToken cancellationToken)
        {
            var spec = new SpaceMemberSpecification(request.Param);
            var result = await _unitOfWork.GetRepository<User, Guid>().GetWithSpecAsync(spec);
            return _mapper.Map<Pagination<UserResponse>>(result);
        }
    }
}