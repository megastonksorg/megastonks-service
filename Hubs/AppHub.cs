using System;
using Microsoft.AspNetCore.SignalR;

namespace Megastonks.Hubs
{
    public class AppHub : Hub
    {
        public async Task JoinGroup(string tribeId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, tribeId);
        }

        public async Task LeaveGroup(string tribeId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, tribeId);
        }
    }
}