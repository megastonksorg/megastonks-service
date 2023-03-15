using System;
using Microsoft.AspNetCore.SignalR;

namespace Megastonks.Hubs
{
	public class AppHub : Hub
	{
        public async Task SendMessage(string userId, string messageId)
        {
            await Clients.All.SendAsync("ReceiveMessage", userId, messageId);
        }
    }
}