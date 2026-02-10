using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trava.Application.Interfaces.Services
{
    public interface IHubNotificationService
    {
        Task SendNotificationToUserAsync(Guid userId, string type, object payload);
        Task SendNotificationToAllAsync(string type, object payload);
        Task SendTaskStatusUpdateAsync(Guid userId, object status);
    }
}
