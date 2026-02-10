using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Trava.Application.Hubs;

namespace Trava.API.Hubs
{
    public class SignalRNotificationHub : INotificationHub
    {
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IHubContext<TaskItemStatusHub> _taskItemStatusHubContext;

        public SignalRNotificationHub(
            IHubContext<NotificationHub> notificationHubContext,
            IHubContext<TaskItemStatusHub> taskItemStatusHubContext)
        {
            _notificationHubContext = notificationHubContext;
            _taskItemStatusHubContext = taskItemStatusHubContext;
        }

        public async Task SendNotificationAsync(Guid userId, string type, object payload)
        {
            await _notificationHubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", type, payload);
        }

        public async Task SendNotificationToAllAsync(string type, object payload)
        {
            await _notificationHubContext.Clients.All.SendAsync("ReceiveNotification", type, payload);
        }

        public async Task SendTaskStatusUpdateAsync(Guid userId, object status)
        {
            await _taskItemStatusHubContext.Clients.User(userId.ToString()).SendAsync("ReceiveTaskStatusUpdate", status);
        }
    }
}
