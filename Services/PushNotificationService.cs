using System;
using Megastonks.Entities;
using Megastonks.Helpers;
using Microsoft.Extensions.Options;

namespace Megastonks.Services
{
    public interface IPushNotificationService
    {
        void SendPush(Account account, string body);
    }

    public class PushNotificationService : IPushNotificationService
    {
        private readonly HttpClient _client;
        private readonly PushNotificationSettings _settings;
        private readonly ILogger<PushNotificationService> _logger;

        public PushNotificationService(IOptions<PushNotificationSettings> settings, ILogger<PushNotificationService> logger)
        {
            _client = new HttpClient();
            _settings = settings.Value;
            _logger = logger;
        }

        public async void SendPush(Account account, string body)
        {
            try
            {

            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
            }
        }
    }
}