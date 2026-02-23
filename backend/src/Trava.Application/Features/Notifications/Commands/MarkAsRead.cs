using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Trava.Application.Common.Exceptions;
using Trava.Application.Interfaces;
using Trava.Application.Features.Notifications.Responses;
using Trava.Domain.Entities;
using Trava.Shared.Enums;

namespace Trava.Application.Features.Notifications.Commands
{
    public record MarkAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;

    public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkAsReadCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
        {
            var userNotificationRepo = _unitOfWork.GetRepository<UserNotification, (Guid TargetUserId, Guid NotificationId)>();
            
            var userNotification = await userNotificationRepo.FirstOrDefaultAsync(un => 
                un.NotificationId == request.NotificationId && un.TargetUserId == request.UserId, 
                cancellationToken: cancellationToken);

            if (userNotification == null)
            {
                throw new AppException(CustomCode.NotificationNotFound, new[] { "Notification not found for this user." });
            }

            if (userNotification.IsRead) return true;

            userNotification.IsRead = true;
            userNotificationRepo.Update(userNotification);
            
            return await _unitOfWork.CommitAsync() > 0;
        }
    }
}
