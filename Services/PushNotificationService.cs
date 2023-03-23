﻿using Megastonks.Entities;
using CorePush;
using Megastonks.Helpers;
using Microsoft.Extensions.Options;
using CorePush.Apple;
using Newtonsoft.Json;

namespace Megastonks.Services
{
    public interface IPushNotificationService
    {
        void SendPushToTribe(Account? userToExclude, Tribe tribe, MessageTag messageTag, string body);
    }

    public class PushNotificationService : IPushNotificationService
    {
        public class AppleNotification
        {
            public class PayloadData
            {
                public string TribeId { get; set; }
                public MessageTag MessageTag { get; set; }
            }

            public class ApsPayload
            {
                public class AlertBody
                {
                    public string Title { get; set; }
                    public string Body { get; set; }
                }

                public AlertBody Alert { get; set; }
            }

            public ApsPayload Aps { get; set; }
            public string TribeId { get; set; }
            public MessageTag MessageTag { get; set; }
        }

        private readonly HttpClient _client;
        private readonly PushNotificationSettings _settings;
        private readonly ILogger<PushNotificationService> _logger;

        public PushNotificationService(IOptions<PushNotificationSettings> settings, ILogger<PushNotificationService> logger)
        {
            _client = new HttpClient();
            _settings = settings.Value;
            _logger = logger;
        }

        public void SendPushToTribe(Account? userToExclude, Tribe tribe, MessageTag messageTag, string body)
        {
            var data = new PushNotificationService.AppleNotification.PayloadData
            {
                TribeId = tribe.Id.ToString(),
                MessageTag = messageTag
            };

            if (userToExclude != null)
            {
                tribe.TribeMembers.RemoveAll(x => x.Account == userToExclude);
            }

            foreach (var member in tribe.TribeMembers)
            {
                SendPush(member.Account, tribe.Name, body, data);
            }
        }

        private async void SendPush(Account account, string title, string body, PushNotificationService.AppleNotification.PayloadData data)
        {
            try
            {
                DeviceType? deviceType = account.DeviceType;
                string? deviceToken = account.DeviceToken;

                if (deviceType != null && deviceToken != null)
                {
                    switch (deviceType)
                    {
                        case DeviceType.apple:
                            var notification = new AppleNotification
                            {
                                Aps = new AppleNotification.ApsPayload
                                {
                                    Alert = new AppleNotification.ApsPayload.AlertBody
                                    {
                                        Title = title,
                                        Body = body
                                    }
                                },
                                TribeId = data.TribeId,
                                MessageTag = data.MessageTag
                            };

                            var settings = new ApnSettings
                            {
                                AppBundleIdentifier = _settings.Apple.AppIdentifier,
                                P8PrivateKey = _settings.Apple.P8Key,
                                P8PrivateKeyId = _settings.Apple.P8KeyId,
                                TeamId = _settings.Apple.TeamId,
                                ServerType = _settings.Server == "Production" ? ApnServerType.Production : ApnServerType.Development
                            };

                            var apn = new ApnSender(settings, _client);
                            await apn.SendAsync(notification, deviceToken);
                            return;

                        case DeviceType.android:
                            return;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
            }
        }
    }
}