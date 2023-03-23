using System;
using Megastonks.Entities;
using Megastonks.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Megastonks.Hubs
{
    public class AppHub : Hub
    {
        private readonly ILogger<AppHub> _logger;
        private readonly DataContext _dataContext;

        public AppHub(ILogger<AppHub> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }

        public async Task SubscribeToTribes(string walletAddress, string signature)
        {
            try
            {
                if (EthereumSigner.IsSignatureValid("SubscribeToTribes", walletAddress, signature)) {
                    var account = _dataContext.Accounts.Where(x => x.WalletAddress == walletAddress).First();
                    if (account != null)
                    {
                        var tribes = _dataContext.Tribes
                            .Include(x => x.TribeMembers)
                            .Where(x => x.TribeMembers.Any(y => y.Account == account))
                            .ToList();

                        foreach(var tribe in tribes)
                        {
                            await Groups.AddToGroupAsync(Context.ConnectionId, tribe.Id.ToString());
                        }
                    }
                }
            }
            catch(Exception e)
            {
                _logger.LogError("Could not Join Group");
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
            }
        }

        public async Task LeaveGroup(string tribeId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, tribeId);
        }
    }
}