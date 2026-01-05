using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Features.TaskItems.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;
using Trava.Shared.Constants;
using Trava.Shared.Enums;

namespace Trava.Application.Features.TaskItems.Queries
{
    public record GetTaskItemByIdQuery(Guid Id) : IRequest<TaskItemResponse>;

    public class GetTaskItemByIdQueryHandler : IRequestHandler<GetTaskItemByIdQuery, TaskItemResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTaskItemByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TaskItemResponse> Handle(GetTaskItemByIdQuery request, CancellationToken cancellationToken)
        {
            var taskItem = await _unitOfWork.GetRepository<TaskItem, Guid>().GetByIdAsync(request.Id) ?? throw new AppException(CustomCode.TaskItemNotFound);
            return _mapper.Map<TaskItemResponse>(taskItem);
        }
    }
}
