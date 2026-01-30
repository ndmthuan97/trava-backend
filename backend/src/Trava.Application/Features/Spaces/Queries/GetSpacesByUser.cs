using AutoMapper;
using MediatR;
using Trava.Application.Common.Models;
using Trava.Application.Features.Spaces.Responses;
using Trava.Application.Features.Spaces.Specifications;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Spaces.Queries;

public record GetSpacesByUserQuery(Guid UserId) : IRequest<List<SpaceResponse>>;

public class GetSpacesByUserQueryHandler : IRequestHandler<GetSpacesByUserQuery, List<SpaceResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSpacesByUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<SpaceResponse>> Handle(GetSpacesByUserQuery request, CancellationToken cancellationToken)
    {
        var spaceRepo = _unitOfWork.GetRepository<Space, Guid>();

        var result = await spaceRepo.FindAsync(
            x => x.CreatedBy == request.UserId ||
            x.Members.Any(sm => sm.UserId == request.UserId),
            cancellationToken: cancellationToken);

        return _mapper.Map<List<SpaceResponse>>(result);
    }
}
