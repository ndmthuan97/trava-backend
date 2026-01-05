using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Trava.Application.Common.Models;
using Trava.Application.Features.Spaces.Responses;
using Trava.Application.Features.Spaces.Specifications;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Spaces.Queries
{
    public record GetSpacesQuery (SpaceSpecParam Param) : IRequest<Pagination<SpaceResponse>>;

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
            return _mapper.Map<Pagination<SpaceResponse>>(result);
        }
    }
}