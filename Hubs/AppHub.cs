using System;
using Megastonks.Entities;
using Megastonks.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Megastonks.Hubs
{
    [Authorize]
    public class AppHub : Hub
    {
        private readonly ILogger<AppHub> _logger;
        private readonly DataContext _dataContext;

        public AppHub(ILogger<AppHub> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }

        public async Task JoinGroup(string tribeId)
        {
            try
            {
                Account account = (Account)Context.GetHttpContext().Items["Account"];
                var tribe = _dataContext.Tribes.Find(Guid.Parse(tribeId));

                if (account != null && tribe != null)
                {
                    if (tribe.TribeMembers.Find(x => x.Account == account) != null)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, tribeId);
                    }
                }
            }
            catch(Exception e)
            {
                _logger.LogError("Could not Join Group");
                _logger.LogError(e.StackTrace);
            }
        }

        public async Task LeaveGroup(string tribeId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, tribeId);
        }
    }
}