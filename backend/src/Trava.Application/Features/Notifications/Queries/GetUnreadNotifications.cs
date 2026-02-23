using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Trava.Application.Features.Notifications.Responses;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Notifications.Queries
{
    public class GetUnreadNotificationsQuery : IRequest<List<NotificationResponse>>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
    }

    public class GetUnreadNotificationsQueryHandler : IRequestHandler<GetUnreadNotificationsQuery, List<NotificationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUnreadNotificationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<NotificationResponse>> Handle(GetUnreadNotificationsQuery request, CancellationToken cancellationToken)
        {
            var userNotificationRepo = _unitOfWork.GetRepository<UserNotification, (Guid TargetUserId, Guid NotificationId)>();

            var unreadNotifications = await userNotificationRepo.GetListAsync(
                predicate: un => un.TargetUserId == request.UserId && !un.IsRead,
                include: q => q.Include(un => un.Notification)
                               .OrderByDescending(un => un.Notification.CreatedAt),
                cancellationToken: cancellationToken
            );

            return _mapper.Map<List<NotificationResponse>>(unreadNotifications);
        }
    }
}
