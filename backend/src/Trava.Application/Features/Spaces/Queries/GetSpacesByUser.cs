using AutoMapper;
using MediatR;
using Trava.Application.Common.Models;
using Trava.Application.Features.Spaces.Responses;
using Trava.Application.Features.Spaces.Specifications;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Domain.Enums;

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

        var items = result.Data.Select(s => {
            var res = _mapper.Map<SpaceResponse>(s);
            
            SpaceRole? role = s.Members.FirstOrDefault(m => m.UserId == request.Param.UserId)?.SpaceRole;
            
            if (s.SpaceType == SpaceType.Personal && s.CreatedBy == request.Param.UserId)
            {
                role = SpaceRole.Owner;
            }
            
            return res with { Role = role };
        }).ToList();

        return new Pagination<SpaceResponse>(result.PageIndex, result.PageSize, (int)result.Count, items);
    }
}
