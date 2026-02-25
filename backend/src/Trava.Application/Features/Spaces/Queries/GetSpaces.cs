using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using System.Threading;
using Trava.Application.Common.Models;
using Trava.Application.Features.Spaces.Responses;
using Trava.Application.Features.Spaces.Specifications;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Domain.Enums;

namespace Trava.Application.Features.Spaces.Queries
{
    public record GetSpacesQuery(SpaceSpecParam Param) : IRequest<Pagination<SpaceResponse>>;

    public class GetSpacesQueryHandler : IRequestHandler<GetSpacesQuery, Pagination<SpaceResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetSpacesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Pagination<SpaceResponse>> Handle(GetSpacesQuery request, CancellationToken cancellationToken)
        {
            var spec = new SpaceSpecification(request.Param);
            var result = await _unitOfWork.GetRepository<Space, Guid>().GetWithSpecAsync(spec);
            
            var items = result.Data.Select(s => {
                var res = _mapper.Map<SpaceResponse>(s);
                
                if (request.Param.UserId.HasValue)
                {
                    SpaceRole? role = s.Members.FirstOrDefault(m => m.UserId == request.Param.UserId.Value)?.SpaceRole;
                    if (s.SpaceType == SpaceType.Personal && s.CreatedBy == request.Param.UserId.Value)
                    {
                        role = SpaceRole.Owner;
                    }
                    res = res with { Role = role };
                }
                
                return res;
            }).ToList();

            return new Pagination<SpaceResponse>(result.PageIndex, result.PageSize, (int)result.Count, items);
        }
    }
}