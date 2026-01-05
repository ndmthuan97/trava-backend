using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Features.Spaces.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Shared.Enums;

namespace Trava.Application.Features.Spaces.Queries
{
    public record GetSpaceByIdQuery(Guid Id) : IRequest<SpaceResponse>;

    public class GetSpaceByIdQueryHandler : IRequestHandler<GetSpaceByIdQuery, SpaceResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetSpaceByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SpaceResponse> Handle(GetSpaceByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.GetRepository<Space, Guid>().GetByIdAsync(request.Id) ?? throw new AppException(CustomCode.SpaceNotFound);
            return _mapper.Map<SpaceResponse>(result);
        }
    }
}