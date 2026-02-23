using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Trava.Application.Interfaces;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Notifications.Commands
{
    public record MarkAllAsReadCommand(Guid UserId) : IRequest<bool>;

    public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkAllAsReadCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
        {
            var userNotificationRepo = _unitOfWork.GetRepository<UserNotification, (Guid TargetUserId, Guid NotificationId)>();
            
            var unreadNotifications = await userNotificationRepo.FindAsync(un => 
                un.TargetUserId == request.UserId && !un.IsRead, 
                cancellationToken);

            if (!unreadNotifications.Any()) return true;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                userNotificationRepo.Update(notification);
            }
            
            return await _unitOfWork.CommitAsync() > 0;
        }
    }
}
