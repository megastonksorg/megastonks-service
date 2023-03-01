using System;

namespace Megastonks.Models.Message
{
    public class MessageResponse
    {
        public class Reaction
        {
            public string SenderWalletAddress { get; set; }
            public string Content { get; set; }
        }

        public string Id { get; set; }
        public string Body { get; set; }
        public string? Caption { get; set; }
        public bool Deleted { get; set; }
        public string Type { get; set; }
        public string SenderWalletAddress { get; set; }
        public string Tag { get; set; }
        public MessageResponse? Context { get; set; }
        public List<MessageKeyModel> Keys { get; set; }
        public List<Reaction>? Reactions { get; set; }
        public DateTime? Expires { get; set; }
        public DateTime TimeStamp { get; set; } 
    }
}