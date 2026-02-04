using AutoMapper;
using MediatR;
using Trava.Application.Common.Models;
using Trava.Application.Features.Spaces.Responses;
using Trava.Application.Features.Spaces.Specifications;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Spaces.Queries;

public record GetSpacesByUserQuery(SpaceSpecParam Param) : IRequest<Pagination<SpaceResponse>>;

public class GetSpacesByUserQueryHandler : IRequestHandler<GetSpacesByUserQuery, Pagination<SpaceResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSpacesByUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Pagination<SpaceResponse>> Handle(GetSpacesByUserQuery request, CancellationToken cancellationToken)
    {
        var spec = new SpaceSpecification(request.Param);
        var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();

        var result = await spaceRepo.GetWithSpecAsync(spec);

        return _mapper.Map<Pagination<SpaceResponse>>(result);
    }
}
