using System;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Megastonks.Entities
{
    [Owned]
    public class MessageKey
    {
        [JsonIgnore]
        public Message Message { get; set; }
        public string PublicKey { get; set; }
        public string EncryptionKey { get; set; }
    }
}