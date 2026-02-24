using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Trava.Application.Common.Models;
using Trava.Application.Features.SpaceInvitations.Responses;
using Trava.Application.Features.SpaceInvitations.Specifications;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;

namespace Trava.Application.Features.SpaceInvitations.Queries
{
    public record GetMyInvitationsQuery(InvitationSpecParam Param) : IRequest<Pagination<SpaceInvitationResponse>>;

    public class GetMyInvitationsQueryHandler : IRequestHandler<GetMyInvitationsQuery, Pagination<SpaceInvitationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetMyInvitationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Pagination<SpaceInvitationResponse>> Handle(GetMyInvitationsQuery request, CancellationToken cancellationToken)
        {
            var spec = new InvitationSpecification(request.Param);
            var result = await _unitOfWork.GetRepository<SpaceInvitation, Guid>().GetWithSpecAsync(spec);

            var items = _mapper.Map<List<SpaceInvitationResponse>>(result.Data);
            return new Pagination<SpaceInvitationResponse>(result.PageIndex, result.PageSize, (int)result.Count, items);
        }
    }
}
