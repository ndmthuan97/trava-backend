using AutoMapper;
using MediatR;
using Trava.Application.Common.Models;
using Trava.Application.Features.Users.Responses;
using Trava.Application.Features.Users.Specifications;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Users.Queries;

public record GetUsersQuery(UserSpecParam param) : IRequest<Pagination<UserResponse>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Pagination<UserResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public GetUsersQueryHandler(IUnitOfWork unitOfWork,  IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<Pagination<UserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var spec = new UserSpecification(request.param);
        var result = await _unitOfWork.GetRepository<User, Guid>().GetWithSpecAsync(spec);
        return _mapper.Map<Pagination<UserResponse>>(result);
    }
}