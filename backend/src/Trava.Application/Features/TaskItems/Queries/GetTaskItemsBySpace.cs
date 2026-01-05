using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Trava.Application.Common.Models;
using Trava.Application.Features.TaskItems.Responses;
using Trava.Application.Features.TaskItems.Specifications;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;

namespace Trava.Application.Features.TaskItems.Queries
{
    public record GetTaskItemsBySpaceQuery(TaskItemSpecParam Param) : IRequest<Pagination<TaskItemResponse>>;

    public class GetTaskItemsBySpaceQueryHandler : IRequestHandler<GetTaskItemsBySpaceQuery, Pagination<TaskItemResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTaskItemsBySpaceQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Pagination<TaskItemResponse>> Handle(GetTaskItemsBySpaceQuery request, CancellationToken cancellationToken)
        {
            var spec = new TaskItemSpecification(request.Param);
            var result = await _unitOfWork.GetRepository<TaskItem, Guid>().GetWithSpecAsync(spec);

            return _mapper.Map<Pagination<TaskItemResponse>>(result);
        }
    }
}
