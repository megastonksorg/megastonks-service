using System;
using Megastonks.Entities;

namespace Megastonks.Models.Message
{
    public class MessageResponse
    {
        public class Reaction
        {
            public string SenderWalletAddress { get; set; }
            public string Content { get; set; }
        }

        public Guid Id { get; set; }
        public string Body { get; set; }
        public string? Caption { get; set; }
        public bool Deleted { get; set; }
        public MessageType Type { get; set; }
        public string SenderWalletAddress { get; set; }
        public MessageTag Tag { get; set; }
        public Guid? Context { get; set; }
        public List<MessageKey> Keys { get; set; }
        public List<Reaction>? Reactions { get; set; }
        public DateTime? Expires { get; set; }
        public DateTime TimeStamp { get; set; } 
    }
}