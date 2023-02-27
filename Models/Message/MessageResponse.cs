using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Megastonks.Models.Message
{
    public class MessageResponse
    {
        public class Key
        {
            public string PublicKey { get; set; }
            public string EncryptionKey { get; set; }
        }

        public class Reaction
        {
            public string SenderWalletAddress { get; set; }
            public string Content { get; set; }
        }

        public string Id { get; set; }
        public MessageContent Content { get; set; }
        public MessageResponse? Context { get; set; }
        public string? SenderWalletAddress { get; set; }
        public string Tag { get; set; }
        public List<Key> Keys { get; set; }
        public List<Reaction>? Reactions { get; set; }
        public DateTime? Expires { get; set; }
        public DateTime TimeStamp { get; set; } 
    }
}