using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Trava.Application.Hubs;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Services;
using Trava.Domain.Entities;

namespace Trava.Infrastructure.Services
{
    public class HubNotificationService : IHubNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationHub _notificationHub;

        public HubNotificationService(IUnitOfWork unitOfWork, INotificationHub notificationHub)
        {
            _unitOfWork = unitOfWork;
            _notificationHub = notificationHub;
        }

        public async Task SendNotificationToUserAsync(Guid userId, string type, object payload)
        {
            // 1. Save to DB
            var notification = new Notification
            {
                Type = type,
                Payload = JsonSerializer.Serialize(payload),
                CreatedAt = DateTimeOffset.UtcNow
            };

            var notificationRepo = _unitOfWork.GetRepository<Notification, Guid>();
            await notificationRepo.AddAsync(notification);

            var userNotificationRepo = _unitOfWork.GetRepository<UserNotification, object>(); // Composite key or similar?
            await userNotificationRepo.AddAsync(new UserNotification
            {
                TargetUserId = userId,
                NotificationId = notification.Id,
                IsRead = false
            });

            await _unitOfWork.CommitAsync();

            // 2. Send via SignalR
            await _notificationHub.SendNotificationAsync(userId, type, payload);
        }

        public async Task SendNotificationToAllAsync(string type, object payload)
        {
            // For sending to all, we might not want to save individual UserNotifications immediately 
            // depending on the requirements. Often "All" notifications are handled differently.
            // But if the user wants persistence, we'd need to link to all users.
            // For now, let's just save the notification itself.

            var notification = new Notification
            {
                Type = type,
                Payload = JsonSerializer.Serialize(payload),
                CreatedAt = DateTimeOffset.UtcNow
            };

            var notificationRepo = _unitOfWork.GetRepository<Notification, Guid>();
            await notificationRepo.AddAsync(notification);
            await _unitOfWork.CommitAsync();

            await _notificationHub.SendNotificationToAllAsync(type, payload);
        }

        public async Task SendTaskStatusUpdateAsync(Guid userId, object status)
        {
            // Path: TaskItemStatusHub
            // Usually status updates for background processes aren't persisted in the Notification table 
            // unless requested. The user said TaskItemStatusHub is "specialized for updating background process progress".
            
            await _notificationHub.SendTaskStatusUpdateAsync(userId, status);
        }
    }
}
