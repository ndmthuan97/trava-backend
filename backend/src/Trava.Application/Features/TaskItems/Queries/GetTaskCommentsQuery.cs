using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trava.Application.Features.TaskItems.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;

namespace Trava.Application.Features.TaskItems.Queries
{
    public record GetTaskCommentsQuery(Guid TaskItemId) : IRequest<List<TaskCommentResponse>>;

    public class GetTaskCommentsQueryHandler : IRequestHandler<GetTaskCommentsQuery, List<TaskCommentResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTaskCommentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<TaskCommentResponse>> Handle(GetTaskCommentsQuery request, CancellationToken cancellationToken)
        {
            var commentRepo = _unitOfWork.GetRepository<TaskComment, Guid>();
            
            var comments = await commentRepo.GetListAsync(
                x => x.TaskItemId == request.TaskItemId,
                q => q.Include(c => c.User),
                cancellationToken
            );
            return _mapper.Map<List<TaskCommentResponse>>(comments.OrderBy(c => c.CreatedAt));
        }
    }
}
