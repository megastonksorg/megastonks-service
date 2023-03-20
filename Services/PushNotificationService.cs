using System;
using Megastonks.Entities;
using Megastonks.Helpers;
using Microsoft.Extensions.Options;

namespace Megastonks.Services
{
    public interface IPushNotificationService
    {
        void SendPush(Account account, string info);
    }

    public class PushNotificationService : IPushNotificationService
    {
        private readonly HttpClient _client;
        private readonly PushNotificationSettings _settings;

        public PushNotificationService(IOptions<PushNotificationSettings> settings)
        {
            _client = new HttpClient();
            _settings = settings.Value; 
        }

        public async void SendPush(Account account, string info)
        {

        }
    }
}