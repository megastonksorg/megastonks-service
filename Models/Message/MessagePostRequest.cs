using System;

namespace Megastonks.Models.Message
{
    public class MessagePostRequest
    {
        public class Key
        {
            public string PublicKey { get; set; }
            public string EncryptionKey { get; set; }
        }

        public class MessageContent
        {
            public string Body { get; set; }
            public string Caption { get; set; }
            public string Type { get; set; }
        }

        public MessageContent Content { get; set; }
        public string ContextId { get; set; }
        public List<Key> Keys { get; set; }
    }
}