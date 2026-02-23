using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Trava.Application.Common.Models;
using Trava.Application.Features.Notifications.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Constants;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Notifications.Queries
{
    public class GetUserNotificationsQuery : IRequest<Pagination<NotificationResponse>>
    {
        public Guid UserId { get; set; }
        public int PageIndex { get; set; } = AppConstants.DEFAULT_PAGE_INDEX;
        public int PageSize { get; set; } = AppConstants.DEFAULT_PAGE_SIZE;
    }

    public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, Pagination<NotificationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserNotificationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Pagination<NotificationResponse>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var userNotificationRepo = _unitOfWork.GetRepository<UserNotification, (Guid TargetUserId, Guid NotificationId)>();

            var totalCount = await userNotificationRepo.CountAsync(un => un.TargetUserId == request.UserId, cancellationToken);
            
            var userNotifications = await userNotificationRepo.GetListAsync(
                un => un.TargetUserId == request.UserId,
                q => q.Include(un => un.Notification)
                    .OrderByDescending(un => un.Notification.CreatedAt)
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize),
                cancellationToken
            );

            var data = _mapper.Map<IReadOnlyCollection<NotificationResponse>>(userNotifications);

            return new Pagination<NotificationResponse>(request.PageIndex, request.PageSize, totalCount, data);
        }
    }
}
