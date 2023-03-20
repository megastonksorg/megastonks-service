using System;
namespace Megastonks.Helpers
{
    public class PushNotificationSettings
    {
        public class AppleSettings
        {
            public string P8Key { get; set; }
            public string P8KeyId { get; set; }
            public string TeamId { get; set; }
            public string AppIdentifier { get; set; }
        }

        public AppleSettings Apple { get; set; }
        public string Server { get; set; }
    }
}