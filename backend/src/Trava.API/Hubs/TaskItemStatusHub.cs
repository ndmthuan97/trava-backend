using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Trava.API.Hubs
{
    [AllowAnonymous]
    public class TaskItemStatusHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[SignalR] TaskItemStatusHub connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
    }
}
